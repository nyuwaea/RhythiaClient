using Godot;
using System;

public partial class PlaytestOverlay : Panel
{
    public bool PlaytestInit = false;
    [Export] public ReplayManager ReplayManager {get; set;}
    public Runner Runner;
    public Attempt Attempt;

    public override void _Ready()
    {
        // Playtest Overlay
        VBoxContainer VBHolder = GetNode<VBoxContainer>("VB");
        LineEdit StartFromEdit = VBHolder.GetNode<HBoxContainer>("StartFrom").GetNode<LineEdit>("LineEdit");
        StartFromEdit.FocusExited += () => { ApplyStartFrom(StartFromEdit.Text, Attempt.Map, StartFromEdit); };
        StartFromEdit.TextSubmitted += (_) => { ApplyStartFrom(StartFromEdit.Text, Attempt.Map, StartFromEdit); };
        VBHolder.GetNode<Button>("PlayButton").Pressed += () => UpdatePlaytestOverlay(false);
    }

    public void UpdatePlaytestOverlay(bool show)
    {

        Runner.Playing = !show;

        SoundManager.Song.PitchScale = (float)Attempt.Speed;
        SoundManager.Song.StreamPaused = !Runner.Playing;

        Visible = show;

        MenuCursor.Instance.UpdateVisible(Visible && SettingsManager.Instance.Settings.UseCursorInMenus.Value);

        if (Visible)
        {
            Input.WarpMouse(GetViewport().GetWindow().Size / 2);
        }
        else
        {
            Input.MouseMode = Attempt.IsReplay && ReplayManager.ViewerVisible ? Input.MouseModeEnum.Visible
                : Attempt.Settings.AbsoluteInput ? Input.MouseModeEnum.ConfinedHidden
                : Input.MouseModeEnum.Captured;
        }

        VBoxContainer VBHolder = GetNode<VBoxContainer>("VB");

        Rhythia.Instance.TempMods["Spin"] = VBHolder.GetNode<CheckButton>("SpinCheck").ButtonPressed;

        LineEdit StartFromEdit = VBHolder.GetNode<HBoxContainer>("StartFrom").GetNode<LineEdit>("LineEdit");
        LineEdit SpeedEdit = VBHolder.GetNode<HBoxContainer>("Speed").GetNode<LineEdit>("LineEdit");

        PlaytestInit = !show;

        if (PlaytestInit == false && SettingsManager.Instance.Settings.OptionalParameters)
        {
            // start from init
            double.TryParse(Rhythia.StartFromParameter, out double sfInit);
            string sfSeconds = (sfInit /= 1000).ToString();
            ApplyStartFrom(sfSeconds, Attempt.Map, StartFromEdit);

            // speed init
            double.TryParse(Rhythia.SpeedParameter, out double spInit);
            SpeedEdit.Text = spInit.ToString();
        }

        if (!show)
        {
            Attempt oldAttempt = Attempt;
            Runner.Stop(false);

            var map = MapParser.Decode(oldAttempt.Map.FilePath, Rhythia.AudioFilePath);

            double SpeedValue = 1.0;
            if (double.TryParse(SpeedEdit.Text, out double speeddouble)) SpeedValue = speeddouble;

            GameScene.Attempt = new Attempt(map, SpeedValue, GetStartFrom(StartFromEdit), Rhythia.Instance.TempMods);
            SceneManager.ReloadCurrentScene();
        }
    }

    public void ApplyStartFrom(string input, Map map, LineEdit valueEdit)
    {
        // Hello MapInfoContainer.cs! :) -fog

        input ??= valueEdit.Text == "" ? valueEdit.PlaceholderText : valueEdit.Text;
        

        if (input.Contains(":")) // time conversion (ex. 1:25)
        {
            if (!input.IsValidFloat())
            {
                valueEdit.Text = Util.String.FormatTime(1.0);
            }

            double value = 0;
            string[] split = input.Split(":");
            split.Reverse();

            if (split.Length > 1 && split[1].IsValidFloat())
            {
                value += 60 * split[1].ToFloat();
            }
            if (double.TryParse(split[0], System.Globalization.CultureInfo.InvariantCulture, out double inputValue))
            {
                if (inputValue < 1) inputValue *= map.Length / 1000;
                value += inputValue;
            }

            value = Math.Clamp(value * 1000, 0, map.Length);

            valueEdit.Text = Util.String.FormatTime(value / 1000);
        }
        else if (!input.Contains(":"))
        {
            if (!input.IsValidFloat())
            {
                valueEdit.Text = Util.String.FormatTime(1.0);
            }

            double value = 0.0;

            if (double.TryParse(input, out double inputValue)) value = inputValue;
            value = Math.Clamp(value, 0, map.Length);

            valueEdit.Text = Util.String.FormatTime(value);
        }
    }

    public double GetStartFrom(LineEdit valueEdit)
    {
        double value = 0;
        string input = valueEdit.Text;

        string[] split = input.Split(":");
        split.Reverse();

        if (split.Length > 1 && split[1].IsValidFloat())
        {
            value += 60 * split[1].ToFloat();
        }

        return value;
    }
}
