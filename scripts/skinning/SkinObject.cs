using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Skinning;

/// <summary>
/// 
/// </summary>
public abstract partial class SkinObject : RefCounted
{
    public enum DecorabilityType
    {
        None,
        Flat,
        Spatial,
        All
    }

    /// <summary>
    /// 
    /// </summary>
    // public string UID { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = "Object";

    /// <summary>
    /// 
    /// </summary>
    public bool Persistent { get; protected set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public DecorabilityType Decorability {
        get;
        protected set { Shadeable = field == DecorabilityType.Flat || field == DecorabilityType.All; }
    } = DecorabilityType.None;

    /// <summary>
    /// 
    /// </summary>
    public bool Shadeable { get; private set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, SkinObjectProperty<Variant>> Properties { get; protected set; } = [];

    /// <summary>
    /// 
    /// </summary>
    public SkinObject Parent { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<SkinObject> Children { get; set; } = [];

    private Regex nameRegex = new("[^a-zA-Z0-9()-]");

    public SkinObject()
    {
        Name = this.GetType().Name;
    }

    public void AddChild(SkinObject child)
    {
        child.Parent = this;

        if (!Children.Exists(x => x == child))
        {
            Children.Add(child);
        }
    }

    public void AddChildren(IEnumerable<SkinObject> children)
    {
        foreach (var child in children)
        {
            AddChild(child);
        }
    }

    public void RemoveChild(SkinObject child)
    {
        child = Children.Find(x => x == child);

        if (child == null || child.Parent != this)
        {
            return;
        }

        child.Parent = null;
        Children.Remove(child);
    }

    public void Rename(string name)
    {
        Name = nameRegex.Replace(name, "_");
    }

    public void Delete()
    {
        if (Persistent)
        {
            throw new("Cannot delete a persistent skin object");
        }


    }
}