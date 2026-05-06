using Godot;
using System;

public partial class SkinEditor : BaseScene
{
	[Export]
	private Button backButton;
	[Export]
	private Label skinPath;
	[Export]
	private LineEdit skinName;

	public override void _Ready()
	{
		base._Ready();
		
		backButton.Pressed += () => {
			SceneManager.Load("res://scenes/main_menu.tscn");
		};
	}

	public override void Load()
	{
		skinPath.Text = $"{Constants.USER_FOLDER}/";
		skinName.Text = "";
	}

	public override void Unload()
	{

	}
}
