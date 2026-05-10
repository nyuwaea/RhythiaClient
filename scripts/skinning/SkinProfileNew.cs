using Skinning.Categories;

namespace Skinning;

/// <summary>
///  
/// </summary>
public partial class SkinProfileNew
{
    public HUD HUD { get; private set; }

    // public SkinCategory Menu { get; private set; }
    
    // public SkinCategory Themes { get; private set; }

    public SkinProfileNew()
    {
        HUD = new();
    }
}