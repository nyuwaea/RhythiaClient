using Godot;

namespace Skinning.Objects;

public partial class Cursor : SkinObject
{
    [Skinnable]
    public Texture2D Image { get; set; } = new();

    [Skinnable]
    public Texture2D TrailImage { get; set; } = new();

    [Skinnable]
    public Vector2 Size { get; set; } = Vector2.One;

    [Skinnable]
    public double Rotation { get; set; } = 0;

    public Cursor()
    {
        Persistent = true;
    }
}