using Skinning.Categories;

namespace Skinning;

/// <summary>
///  
/// </summary>
public partial class SkinProfileNew
{
    public string Name { get; set; }

    public string Path { get; set; }

    public HUD HUD { get; private set; }

    // public SkinCategory Menu { get; private set; }
    
    // public SkinCategory Themes { get; private set; }

    public SkinProfileNew(string name)
    {
        Name = name ?? "Skin";
        HUD = new();
    }

    public override string ToString() => Name;
}