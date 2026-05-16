using Godot;
using Skinning;

public partial class SkinExplorerItem : HBoxContainer
{
	public SkinObject SkinObject;

	public SkinExplorerItem Parent;

	public bool Collapsed = false;

	[Signal]
	public delegate void SelectedEventHandler();

	[Export]
	private HBoxContainer infoContainer;

	[Export]
	private Button select;

	[Export]
	private Label label;

	[Export]
	private TextureRect icon;

	[Export]
	private Button add;

	[Export]
	private Button delete;

	[Export]
	private VBoxContainer childrenContainer;

	[Export]
	private Control hoverHighlight;

	[Export]
	private Control selectHighlight;

	[Export]
	private Control hierarchyContainer;

	[Export]
	private Button collapse;

	[Export]
	private Control lineContainer;

	[Export]
	private Control lineVertical;

	[Export]
	private Button lineButton;

	[Export]
	private Texture2D foldIcon;

	[Export]
	private Texture2D unfoldIcon;

	private readonly PackedScene propertyTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/ui/skin_editor/skin_property_item.tscn");

	public override void _Ready()
	{
		infoContainer.MouseEntered += () => updateHover(true);
		infoContainer.MouseExited += () => updateHover(false);

		select.Pressed += () => EmitSignal(SignalName.Selected);

		void setupCollapseButton(Button button)
		{
			button.MouseEntered += () => { hierarchyContainer.Modulate = new(0xffffffff); };
			button.MouseExited += () => { hierarchyContainer.Modulate = new(0xffffff40); };
			button.Pressed += () => CollapseChildren(!Collapsed);
		}

		setupCollapseButton(collapse);
		setupCollapseButton(lineButton);

		// Add.Pressed += 
		// Delete.Pressed +=
	}

	public void SetObject(SkinObject skinObject)
	{
		SkinObject = skinObject;
		
		Name = skinObject.GUID.ToString();
		label.Text = skinObject.Name;
		icon.Texture = skinObject.Icon;
		add.Visible = skinObject.Decorability != SkinObject.DecorabilityType.None;
		delete.Visible = !skinObject.Persistent;

		UpdateHierarchy();
	}

	public void AddChildItem(SkinExplorerItem item)
	{
		childrenContainer.AddChild(item);
		item.Parent = this;

		CollapseChildren(false);
		CallDeferred("UpdateHierarchy");
	}

	public void RemoveChildItem(SkinExplorerItem item)
	{
		childrenContainer.RemoveChild(item);
		item.Parent = null;

		UpdateHierarchy();
	}

	public async void CollapseChildren(bool toggle)
	{
		Collapsed = toggle;

		childrenContainer.Visible = !toggle;
		lineContainer.Visible = !toggle;
		collapse.Icon = toggle ? unfoldIcon : foldIcon;

		if (Parent != null)
		{
			// bruh
			await ToSignal(Rhythia.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);

			Parent.UpdateHierarchy();
		}
	}

	public void UpdateHierarchy()
	{
		var children = childrenContainer.GetChildren();
		bool isEmpty = children.Count == 0;

		collapse.Disabled = isEmpty;
		collapse.SelfModulate = new(isEmpty ? 0xffffff00 : 0xffffffff);
		collapse.MouseDefaultCursorShape = isEmpty ? CursorShape.Arrow : CursorShape.PointingHand;
		lineButton.Disabled = isEmpty;
		lineButton.MouseDefaultCursorShape = collapse.MouseDefaultCursorShape;
		
		if (!isEmpty)
		{
			var last = (Control)children[^1];

			lineVertical.CustomMinimumSize = new(0, last.Position.Y + CustomMinimumSize.Y / 2);
		}
		else
		{
			CollapseChildren(true);
		}
	}

	public void Select(bool selected = true)
	{
		selectHighlight.Visible = selected;
	}

	public void Deselect()
	{
		Select(false);
	}

	private void updateHover(bool toggle)
	{
		hoverHighlight.Visible = toggle;
	}
}
