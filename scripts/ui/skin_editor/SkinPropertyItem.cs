using Godot;
using System;

public partial class SkinPropertyItem : VBoxContainer
{
	public string Type;

	[Export]
	private Label label;

	[Export]
	private HBoxContainer valueContainer;

	public override void _Ready()
	{

	}

	public void SetProperty(string name, string type)
	{
		label.Text = name;
		Type = type;
	}
}
