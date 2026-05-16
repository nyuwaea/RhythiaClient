using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Skinning;

public partial class SkinEditor : BaseScene
{
	public static SkinEditor Instance;

	/// <summary>
	/// Currently selected skin for editing.
	/// </summary>
	public SkinProfileNew Skin;

	[ExportGroup("Panels")]

	[Export]
	public SkinExplorer Explorer;

	[Export]
	public SkinProperties Properties;

	// UI template scenes
	private readonly PackedScene skinSelectTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/ui/skin_editor/skin_select.tscn");

	[ExportGroup("Header")]

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

	[ExportGroup("Selection")]

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

	[ExportGroup("Docks")]

	// Docks
	[Export]
	private HSplitContainer dockSplit;
	[Export]
	private VSplitContainer objectsSplit;

	public override void _Ready()
	{
		base._Ready();

		Instance = this;

		backButton.Pressed += () => exit();

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
		skinName.FocusExited += () => { renameSelected(skinName.Text == "" ? Skin.Name : skinName.Text); };

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

		SkinProfileNew newSkin = new(name);

		select(newSkin);
	}

	private void select(SkinProfileNew skin = null)
	{
		Skin = skin;

		skinName.Text = Skin?.Name ?? "";
		skinName.PlaceholderText = Skin?.Name ?? "(no skin selected)";

		toggleSelection(Skin == null);

		if (Skin != null)
		{
			Explorer.BuildCategory(Skin.HUD);
			Properties.ClearProperties();
		}
	}

	private void save()
	{

	}

	private void share()
	{
		
	}

	private void exit()
	{
		Explorer.ClearCategory(false);
		Properties.ClearProperties(false);

		// TODO: Check and prompt unsaved changes
		SceneManager.Load("res://scenes/main_menu.tscn");
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
		if (Skin == null)
		{
			return;
		}
		
		(int _, string name) = validateSkinName(newName);

		Skin.Name = name;
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
