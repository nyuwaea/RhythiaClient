using Godot;

public partial class SkinEditorButton : Button
{
    public override void _Ready()
    {
        Pressed += () => { SceneManager.Load("res://scenes/skin_editor.tscn"); };
    }
}
