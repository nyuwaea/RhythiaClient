using Godot;
using System;
using System.Collections.Generic;

public partial class GameScene : BaseScene
{
	[Export] public Runner Runner;
	[Export] public Panel Menu;
	[Export] public ReplayManager ReplayManager { get; private set; }
	[Export] public PlayerInputController PlayerInputController { get; private set; }
    [Export] public CursorManager CursorManager { get; private set; }

	[Signal] public delegate void StartTempPauseEventHandler(Attempt attempt);

	public static Attempt Attempt;
	public static bool StartQueued = false;

	public bool MenuShown = false;

	public static GameScene Instance;

	public override void _Ready()
	{
		base._Ready();

		Instance = this;

		ReplayManager ??= GetNode<ReplayManager>("ReplayManager");
        CursorManager ??= GetNode<CursorManager>("CursorManager");
        PlayerInputController ??= GetNode<PlayerInputController>("PlayerInputController");

        if (CursorManager == null)
            Logger.Error("No CursorManager found!");
        if (PlayerInputController == null)
            Logger.Error("No PlayerInputController found!");

        PlayerInputController.OnMouseMove += (relative, absolute) =>
        {
            if (!Runner.Playing || Attempt.IsReplay) return;

            if (Attempt.Settings.AbsoluteInput)
            {
                // Take mouse position difference between center of the current window size
                // This is to make the mouse position the same as relative if it was locked, or confined
                Vector2 absolutePosition = absolute - (GetViewport().GetWindow().Size / 2);

                // Multiply by 0.582f to make it 1:1 to absolute scale on nightly
                CursorManager.UpdateCursor(absolutePosition * 0.582f);
            }
            else
            {
                CursorManager.UpdateCursor(relative);
            }
            Attempt.DistanceMM += relative.Length() / Attempt.Settings.Sensitivity / 57.5;
        };

		PlayerInputController.OnLeftMouseButton += isPressed =>
		{
			if (!isPressed) return;

			ReplayManager.LMB = isPressed;
		};

		PlayerInputController.OnTogglePaused += () =>
		{
			Attempt.Qualifies = false;

			if (SettingsManager.Shown)
			{
				SettingsMenu.Instance.HideMenu();
			}
			else
			{
				ShowMenu(!MenuShown);
			}
		};

		PlayerInputController.OnToggleReplayViewerVisibility += () =>
		{
			if (Attempt.IsReplay)
			{
				ReplayManager.ShowReplayViewer(Attempt);
			}
		};

		PlayerInputController.OnPauseOrSkipPressed += () =>
		{
			if (Attempt.IsReplay)
			{
				Runner.Playing = !Runner.Playing;
				SoundManager.Song.PitchScale = (float)Attempt.Speed;
				SoundManager.Song.StreamPaused = !Runner.Playing;

				string texturePath = Runner.Playing
                    ? "res://textures/ui/pause.png"
                    : "res://textures/ui/play.png";

				ReplayManager.SeekerPause.TextureNormal = GD.Load<Texture2D>(texturePath);
			}
			else
			{
				if (Lobby.Players.Count > 1) return;
				Runner.Skip();

				// Space To Pause
				// if (!Attempt.CanSkip && Attempt.Settings.SpaceToPause)
				// {
				// 	EmitSignal(SignalName.StartTempPause, Attempt);
				// }
			}
		};

		// PlayerInputController.OnPauseOrSkipReleased += () =>
		// {
			
		// };

		PlayerInputController.OnToggleFade += () => Attempt.Settings.FadeOut.Value = Attempt.Settings.FadeOut.Value > 0 ? 0 : 100;
		PlayerInputController.OnTogglePushback += () => Attempt.Settings.Pushback.Value = !Attempt.Settings.Pushback;
		PlayerInputController.OnRestartPressed += Restart;

		Panel menuButtonsHolder = Menu.GetNode<Panel>("Holder");

		Menu.GetNode<Button>("Button").Pressed += HideMenu;
		menuButtonsHolder.GetNode<Button>("Resume").Pressed += HideMenu;
		menuButtonsHolder.GetNode<Button>("Restart").Pressed += Restart;
		menuButtonsHolder.GetNode<Button>("Settings").Pressed += () => {
			SettingsMenu.Instance.ShowMenu();
		};
		menuButtonsHolder.GetNode<Button>("Quit").Pressed += () => {
			if (Attempt.Alive)
			{
				SoundManager.FailSound.Play();
			}

			Attempt.Alive = false;
			Attempt.Qualifies = false;

			if (Attempt.DeathTime == -1)
			{
				Attempt.DeathTime = Math.Max(0, Attempt.Progress);
			}

			Runner.Stop();
		};
	}

	public override void Load()
	{
		base.Load();

		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

		MenuCursor.Instance.UpdateVisible(false, false);
		SceneManager.Space.UpdateState(true);
		SceneManager.Space.UpdateMap(Attempt.Map);

		Control focused = SceneManager.Root.GetViewport().GuiGetFocusOwner();
		focused?.ReleaseFocus();
		Input.MouseMode = Attempt.Settings.AbsoluteInput.Value || Attempt.IsReplay ? Input.MouseModeEnum.ConfinedHidden : Input.MouseModeEnum.Captured;
		Input.UseAccumulatedInput = false;

		Runner.Attempt = Attempt;
		ReplayManager.InitReplayLength();

		if (Runner.Attempt.IsReplay)
		{
			ReplayManager.CurrentMode = ReplayManager.Mode.PLAYBACK;
		}
		else if (Runner.Attempt.Settings.RecordReplays)
		{
			ReplayManager.NewReplay(Runner.Attempt);
			ReplayManager.CurrentMode = ReplayManager.Mode.RECORD;
		}
		else
		{
			ReplayManager.CurrentMode = ReplayManager.Mode.NONE;
		}

		Logger.Log($"Replay Mode: {ReplayManager.CurrentMode}");

		HideMenu();

		Runner.Play();
		StartQueued = false;
	}

	public static void Play(Map map, double speed, double startFrom, Dictionary<string, bool> mods, string[] players = null, Replay[] replays = null)
	{
		if (StartQueued) return;

		StartQueued = true;

		var parsedMap = MapParser.Decode(map.FilePath);
		Attempt = new Attempt(parsedMap, speed, startFrom, mods ?? [], players, replays);

		if (!Attempt.IsReplay)
		{
			Stats.Instance.Attempts++;
			map.PlayCount++;
			MapManager.Update(map);
		}
		
		SceneManager.Load("res://scenes/game.tscn");
	}

	public void Restart()
	{
		Attempt.Alive = false;
		Attempt.Qualifies = false;
		Runner.Stop(false);

		Attempt oldAttempt = Attempt;

		if (!FileAccess.FileExists(Attempt.ReplayPath) && !Attempt.IsReplay)
		{
			_ = ToastNotification.Notify("Replay desync detected! Sum didn't match notes hit", 2);
		}

		var map = MapParser.Decode(oldAttempt.Map.FilePath);
		Attempt = new Attempt(map, oldAttempt.Speed, oldAttempt.StartFrom, oldAttempt.Mods, oldAttempt.Players, oldAttempt.Replays);

		SceneManager.ReloadCurrentScene();
	}

	public void ShowMenu(bool show = true)
	{
		MenuShown = show;
		Runner.Playing = !MenuShown;

		// rest in peace 0.000000000000000001f pitch scale -fog
		SoundManager.Song.PitchScale = (float)Attempt.Speed;
		SoundManager.Song.StreamPaused = !Runner.Playing;

		MenuCursor.Instance.UpdateVisible(MenuShown && SettingsManager.Instance.Settings.UseCursorInMenus.Value);

		if (MenuShown)
		{
			Menu.Visible = true;
			Input.WarpMouse(GetViewport().GetWindow().Size / 2);
		}
		else
		{
			// Re-sync the audio just in case
			// Already done when the map is running

			// double currentTime = Math.Max(0, (Attempt.Progress - Attempt.Settings.LocalOffset) / 1000.0f);

			// if (Attempt.Map.AudioBuffer != null && SoundManager.Song.Playing)
			// {
			// 	double desyncOffsetTime = Math.Abs(currentTime - SoundManager.Song.GetPlaybackPosition());

			// 	if (desyncOffsetTime > 0.05f)
			// 	{
			// 		Logger.Log($"[color=yellow]Desync detected! Offset by [b]{desyncOffsetTime:F5} seconds![/b][/color]");
			// 		SoundManager.Song.Seek((float)currentTime);
			// 	}
			// }

			Input.MouseMode = Attempt.Settings.AbsoluteInput || Attempt.IsReplay ? Input.MouseModeEnum.ConfinedHidden : Input.MouseModeEnum.Captured;
		}

		Tween tween = Menu.CreateTween();
		tween.TweenProperty(Menu, "modulate", Color.Color8(255, 255, 255, (byte)(MenuShown ? 255 : 0)), 0.25).SetTrans(Tween.TransitionType.Quad);
		tween.TweenCallback(Callable.From(() => {
			Menu.Visible = MenuShown;
		}));
		tween.Play();
	}

	public void HideMenu()
	{
		ShowMenu(false);
	}
}
