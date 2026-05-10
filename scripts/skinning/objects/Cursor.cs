using Godot;

namespace Skinning.Objects;

public partial class Cursor : SkinObject
{
    public Cursor()
    {
        Persistent = true;
        Properties = new() {
            ["Size"] = new(Vector2.One)
        };
    }
}