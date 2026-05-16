using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Skinning;

/// <summary>
/// 
/// </summary>
public abstract partial class SkinObject : RefCounted
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SkinnableAttribute : Attribute { }

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
    public Guid GUID { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = "Object";

    /// <summary>
    /// 
    /// </summary>
    public Texture2D Icon { get; private set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public bool Persistent { get; protected set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public DecorabilityType Decorability {
        get;
        protected set { field = value; Shadeable = value == DecorabilityType.Flat || value == DecorabilityType.All; }
    } = DecorabilityType.None;

    /// <summary>
    /// 
    /// </summary>
    public bool Shadeable { get; private set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public SkinObject Parent { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<SkinObject> Children { get; set; } = [];

    public SkinObject()
    {
        Name = GetType().Name;

        string iconPath = $"res://textures/ui/skinning/{Name.ToSnakeCase()}.png";

        if (ResourceLoader.Exists(iconPath))
        {
            Icon = ResourceLoader.Load<Texture2D>(iconPath);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddChild(SkinObject child)
    {
        child.Parent?.RemoveChild(child);
        child.Parent = this;

        if (!Children.Exists(x => x == child))
        {
            Children.Add(child);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddChildren(IEnumerable<SkinObject> children)
    {
        foreach (var child in children)
        {
            AddChild(child);
        }
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public void Rename(string name)
    {
        Regex nameRegex = new("[^a-zA-Z0-9()-]");
        
        Name = nameRegex.Replace(name, "_");
    }

    /// <summary>
    /// 
    /// </summary>
    public void Delete()
    {
        if (Persistent)
        {
            throw new("Cannot delete a persistent skin object");
        }


    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerable<PropertyInfo> GetProperties()
    {
        return GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(SkinnableAttribute)));
    }
}