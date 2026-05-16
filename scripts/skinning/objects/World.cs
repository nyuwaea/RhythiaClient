namespace Skinning.Objects;

public partial class World : SkinObject
{
    public Grid Grid { get; private set; } = new();

    public Notes Notes { get; private set; } = new();

    public Cursor Cursor { get; private set; } = new();

    public World()
    {
        Persistent = true;
        Decorability = DecorabilityType.Spatial;

        AddChildren([ Grid, Notes, Cursor ]);
    }
}