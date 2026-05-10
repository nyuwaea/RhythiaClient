using Godot;

namespace Skinning;

/// <summary>
/// 
/// </summary>
public class SkinObjectProperty<[MustBeVariant] T>
{
    public Variant Value { get; set; }

    public SkinObjectProperty(T value)
    {
        Value = Variant.From(value);
    }

    public static implicit operator T(SkinObjectProperty<T> x) => x.Value.As<T>();
    public override string ToString() => (string)this.Value;
}