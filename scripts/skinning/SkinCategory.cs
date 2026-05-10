using System.Collections.Generic;

namespace Skinning;

/// <summary>
/// 
/// </summary>
public abstract class SkinCategory
{
    public string Name = "SkinCategory";

    public List<SkinObject> Objects = [];

    public SkinCategory()
    {
        Name = this.GetType().Name;
    }
}