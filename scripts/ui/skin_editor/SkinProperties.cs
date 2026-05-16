using System.Collections.Generic;
using System.Reflection;
using Godot;
using Skinning;

public partial class SkinProperties : Panel
{
	/// <summary>
	/// Currently selected SkinObject to edit properties of.
	/// </summary>
	public SkinObject SkinObject;

	[Export]
	private VBoxContainer itemContainer;

	private Dictionary<string, Stack<SkinPropertyItem>> itemCache = [];

	private List<SkinPropertyItem> items = [];

	private readonly PackedScene itemTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/ui/skin_editor/skin_property_item.tscn");

	public override void _Ready()
	{

	}

    /// <summary>
    /// 
    /// </summary>
	public void BuildProperties(SkinObject skinObject)
	{
		if (SkinObject == skinObject)
		{
			return;
		}

		SkinObject = skinObject;

		ClearProperties();

		if (skinObject != null)
		{
			foreach (var p in skinObject.GetProperties())
			{
				BuildProperty(p);
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public void ClearProperties(bool cache = true)
	{
		foreach(var item in items)
		{
			if (cache)
			{
				itemCache[item.Type].Push(item);
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
	public SkinPropertyItem BuildProperty(PropertyInfo property)
	{
		string type = property.PropertyType.Name;
		var item = createItem(type);

		item.SetProperty(property.Name, type);
		items.Add(item);
		itemContainer.AddChild(item);

		return item;
	}

	private SkinPropertyItem createItem(string type)
	{
        if (!itemCache.TryGetValue(type, out var typeCache))
        {
            typeCache = [];
            itemCache[type] = typeCache;
        }

        if (typeCache.TryPop(out var item))
		{
			return item;
		}

		item = itemTemplate.Instantiate<SkinPropertyItem>();



		return item;
	}
}
