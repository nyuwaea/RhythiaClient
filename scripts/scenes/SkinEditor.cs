using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public partial class SkinEditor : BaseScene
{
	public string Selected;

	// UI template scenes
	private readonly PackedScene skinSelectTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/ui/skin_editor/skin_select.tscn");

	// Header
	[Export]
	private Button backButton;
	[Export]
	private MenuButton importAsset;
	[Export]
	private Button selectButton;
	[Export]
	private Button saveButton;
	[Export]
	private Button shareButton;
	[Export]
	private Label skinPath;
	[Export]
	private LineEdit skinName;
	[Export]
	private HBoxContainer categories;

	// Skin selection
	[Export]
	private ColorRect selectionHolder;
	[Export]
	private Button selectionNewButton;
	[Export]
	private Button selectionImportButton;
	[Export]
	private FileDialog importDialog;
	[Export]
	private VBoxContainer selectionSkins;

	// Docks
	[Export]
	private HSplitContainer dockSplit;
	[Export]
	private VSplitContainer objectsSplit;

	public override void _Ready()
	{
		base._Ready();

		backButton.Pressed += () => {
			// TODO: Check and prompt unsaved changes
			SceneManager.Load("res://scenes/main_menu.tscn");
		};

		var importAssetPopup = importAsset.GetPopup();

		importAssetPopup.IndexPressed += i => {
			switch (importAssetPopup.GetItemText((int)i).ToLower())
			{
				case "image":
					break;
				case "font":
					break;
				case "shader":
					break;
			}
		};

		selectButton.Pressed += () => { select(); };
		saveButton.Pressed += save;
		shareButton.Pressed += share;

		skinName.TextChanged += input => {
			(int result, string _) = validateSkinName(input);

			switch (result)
			{
				default:
					skinName.RemoveThemeColorOverride("font_color");
					break;
				case 1:
					skinName.AddThemeColorOverride("font_color", new(0xe6c629ff));
					break;
				case 2:
					skinName.AddThemeColorOverride("font_color", new(0xe62929ff));
					break;
			}
		};
		skinName.TextSubmitted += _ => skinName.ReleaseFocus();
		skinName.FocusExited += () => { renameSelected(skinName.Text == "" ? Selected : skinName.Text); };

		foreach (Button category in categories.GetChildren())
		{
			// category.Pressed += () => {  };
		}

		selectionNewButton.Pressed += create;
		selectionImportButton.Pressed += importDialog.Show;
		// importDialog.FileSelected += file => {  };
	}

	public override void Load()
	{
		skinPath.Text = $"{Constants.USER_FOLDER}/skins/";

		var viewportSize = (Vector2I)GetViewport().GetVisibleRect().Size;

		dockSplit.SplitOffsets = [viewportSize.X / 3];
		objectsSplit.SplitOffsets = [viewportSize.Y / 2];

		select();
		refreshSelection();
	}

	public override void Unload()
	{

	}

	private void create()
	{
		(int _, string name) = validateSkinName("new-skin");

		select(name);
	}

	private void select(string skin = null)
	{
		Selected = skin;

		skinName.Text = Selected ?? "";
		skinName.PlaceholderText = Selected ?? "(no skin selected)";

		toggleSelection(Selected == null);
	}

	private void save()
	{

	}

	private void share()
	{
		
	}

	private void toggleSelection(bool toggle)
	{
		selectionHolder.Visible = toggle;
		selectButton.Disabled = toggle;
		saveButton.Disabled = toggle;
		shareButton.Disabled = toggle;
		skinName.Editable = !toggle;
	}

	private void refreshSelection()
	{
		List<string> skins = [];

		// Legacy skin compatibility
		foreach (string skinDir in DirAccess.GetDirectoriesAt($"{Constants.USER_FOLDER}/skins"))
		{
			// skins.Add(skinDir.GetBaseName());
		}

		foreach (string skinFile in DirAccess.GetFilesAt($"{Constants.USER_FOLDER}/skins"))
		{
			string ext = skinFile.GetFile().GetExtension();

			if (ext != "rhys")
			{
				continue;
			}


		}

		foreach (var skinSelect in selectionSkins.GetChildren())
		{
			if (!skins.Exists(x => x.Equals(skinSelect.Name)))
			{
				skinSelect.QueueFree();
			}
		}

		foreach (string skin in skins)
		{
			// TODO: Read skin metadata (check if editable/deletable)

			var skinSelect = skinSelectTemplate.Instantiate<SkinSelect>();

			skinSelect.Name = skin;
			skinSelect.Label.Text = skin;
			// Disable Edit and Delete buttons if needed here

			selectionSkins.AddChild(skinSelect);
		}
	}

	private void renameSelected(string newName)
	{
		if (Selected == null)
		{
			return;
		}

		(int _, string name) = validateSkinName(newName);

		Selected = name;
		skinName.Text = name;
		skinName.PlaceholderText = name;
		skinName.EmitSignal(LineEdit.SignalName.TextChanged, name);
	}

	private (int, string) validateSkinName(string name)
	{
		name = new Regex("[^a-zA-Z0-9()-]").Replace(name, "_");

		bool exists = File.Exists($"{Constants.USER_FOLDER}/skins/{name}.rhys");

		if (!exists)
		{
			return (0, name);
		}

		for (int i = 1; i < 100; i++)
		{
			string newName = $"{name}-{i}";

			if (!File.Exists($"{Constants.USER_FOLDER}/skins/{newName}.rhys"))
			{
				return (1, newName);
			}
		}

		return (2, $"{name}-{(long)Time.GetUnixTimeFromSystem()}");
	}
}
