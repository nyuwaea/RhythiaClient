using Godot;
using Skinning;
using System.Collections.Generic;

public partial class SkinExplorer : Panel
{
	/// <summary>
	/// Currently selected category.
	/// </summary>
	public SkinCategory Category;

	/// <summary>
	/// Currently selected SkinExplorerItem.
	/// </summary>
	public SkinExplorerItem SelectedItem;

	[Export]
	private VBoxContainer itemContainer;

	private Stack<SkinExplorerItem> itemCache = [];

	private List<SkinExplorerItem> items = [];

	private readonly PackedScene itemTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/ui/skin_editor/skin_explorer_item.tscn");

    /// <summary>
    /// 
    /// </summary>
	public void BuildCategory(SkinCategory skinCategory)
	{
		if (Category == skinCategory)
		{
			return;
		}
		
		ClearCategory();

		Category = skinCategory;
		
		foreach (var skinObject in skinCategory.Objects)
		{
			BuildObject(skinObject);
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public void ClearCategory(bool cache = true)
	{
		SelectedItem = null;
		
		foreach (var item in items)
		{
			if (cache)
			{
				itemCache.Push(item);
				item.GetParent()?.RemoveChild(item);
			}
			else
			{
				item.QueueFree();
			}
		}

		items = [];
	}

    /// <summary>
    /// 
    /// </summary>
	public SkinExplorerItem BuildObject(SkinObject skinObject, SkinExplorerItem parent = null)
	{
		var item = createItem();
		
		item.SetObject(skinObject);
		items.Add(item);
		
		if (parent == null)
		{
			itemContainer.AddChild(item);
		}
		else
		{
			parent.AddChildItem(item);
		}

		foreach (var child in skinObject.Children)
		{
			BuildObject(child, item);
		}

		return item;
	}

	private SkinExplorerItem createItem()
	{
		if (!itemCache.TryPop(out var item))
		{
			item = itemTemplate.Instantiate<SkinExplorerItem>();
			
			item.Selected += () => {
				SelectedItem?.Deselect();

				SelectedItem = item;
				item.Select();

				SkinEditor.Instance.Properties.BuildProperties(item.SkinObject);
			};
		}

		return item;
	}
}
