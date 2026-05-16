using Godot;

namespace Skinning.Objects;

public partial class Cursor : SkinObject
{
    [Skinnable]
    public Vector2 Size { get; set; } = Vector2.One;

    public Cursor()
    {
        Persistent = true;
    }
}