using Skinning.Objects;

namespace Skinning.Categories;

public partial class HUD : SkinCategory
{
    public Screen Screen { get; private set; } = new();

    public World World { get; private set; } = new();

    public HUD()
    {
        Objects = [ Screen, World ];
    }
}