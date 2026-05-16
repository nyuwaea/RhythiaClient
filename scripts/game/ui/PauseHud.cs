// using Godot;
// using System;
// using System.Reflection.Metadata.Ecma335;

// public partial class PauseHud : UIComponent
// {

// 	[Export] private Label pauseCount;
// 	[Export] private Label pauseTitle;

// 	private Sprite3D pauseHud;
// 	private Control pauseControl;
//     private Control progressMask;

// 	// Time saved at time of initially pausing
// 	private double timePausedAt = 0;
// 	// Cooldown after pause ends, this exists to prevent pause spamming
// 	private float pauseCooldown = 0;

// 	/*
// 	-1 = paused
// 	1 = unpausing
// 	0 = playing/unpaused
// 	*/
// 	private float pauseState = 0;
// 	private bool spaceHeld = false;
// 	private float pauseHoldTime = 0;
// 	private int spacePauses = 0;
// 	private float pauseHudOpacity = 0.75f;

// 	private static readonly Color pause_counter_color = Color.Color8(255, 255, 255);


// 	public override void Init()
// 	{
// 		pauseControl = GetNode<SubViewport>("PauseVP").GetNode<Control>("Control");
// 		// progressMask = GetNode<Control>("ProgressMask");
// 		progressMask = pauseControl.GetNode<Control>("ProgressMask");
// 		SetProgress(0);

// 		GameScene.Instance.StartTempPause += OnStartTempPause;

// 		pauseControl.Visible = false;
// 	}

// 	public void SetProgress(float percent)
// 	{
// 		if (progressMask == null)
// 		{
// 			return;
// 		}

// 		float clamped = Mathf.Clamp(percent, 0f, 1f);
// 		float width = 600f * clamped;
// 		progressMask.Position = new Vector2(300f - (width / 2f), 0f);
// 		progressMask.Size = new Vector2(width, 600f);
// 	}

// 	// sorry cake, but literally what else do i name this -fog
// 	public void EnableCounterOnFirstPause()
// 	{

// 		pauseTitle.Visible = true;
// 		pauseCount.Visible = true;

// 		pauseTitle.Modulate = pause_counter_color;
// 		pauseCount.Modulate = pause_counter_color;

// 	}

// 	public void UpdatePauseHud()
// 	{

// 		// if (pauseHud == null || pauseControl == null)
// 		// {
// 		// 	return;
// 		// }

// 		if (spacePauses > 0 && !Runner.Attempt.Settings.SimpleHUD && !Runner.Attempt.Settings.SuperSimpleHUD)
// 		{
// 			EnableCounterOnFirstPause();
// 		}

// 		pauseCount.Text = spacePauses.ToString();

// 		float maxPercent = pauseState == -1f ? 0f : 1f;
// 		float percent = Math.Clamp(1f - pauseState, 0f, maxPercent);
// 		// bool hideOverlay = isPaused() && Input.IsPhysicalKeyPressed(Key.C);
// 		pauseControl.Visible = pauseState != 0f;
// 		pauseControl.Modulate = new Color(1, 1, 1, Mathf.Abs(pauseState) * pauseHudOpacity);
// 		SetProgress(percent);
// 	}

// 	public void OnStartTempPause(Attempt attempt)
// 	{

// 		if (!isPauseable())
// 		{
// 			return;
// 		}

// 		timePausedAt = attempt.Progress;
// 		pauseState = -1;
// 		Runner.Playing = false;
// 		spaceHeld = false;
// 		pauseHoldTime = 0;
// 		spacePauses++;

// 		attempt.Qualifies = false;
// 		SetProgress(0);

// 		UpdatePauseHud();

// 		if (attempt.Map.AudioBuffer != null && SoundManager.Song.Playing)
//         {
//             SoundManager.Song.Stop();
//         }
// 	}

// 	public void OnStartUnpause()
// 	{
		
// 	}

// 	public void OnStopUnpause()
// 	{
		
// 	}

// 	public void OnCompleteTempPause()
// 	{
		
// 	}

// 	private bool isPaused() => pauseState < 0;

// 	private bool isPauseable()
// 	{
// 		if (Runner.Attempt.IsReplay) return false;

// 		if (GameScene.Instance.MenuShown) return false;

// 		// if (pauseCooldown <= 0) return false;

// 		if (!Runner.Attempt.Settings.SpaceToPause) return false;

// 		// Only allow pausing after 1 second of the map has started playing
// 		if (Runner.Attempt.Progress <= 1000f * Runner.Attempt.Speed) return false;

// 		// Disallow pausing after map is done
// 		if (Runner.Attempt.Progress >= Runner.Attempt.MapLength) return false;

// 		return true;
// 	}



// }
