using System;
using Godot;

public partial class SortMenuButton : Button
{
    [Export]
    private Control panel;

    [Export]
    private Button order;

    [Export]
    private Label orderLabel;

    [Export]
    private VBoxContainer buttons;

    private Button previousButton;

    public override void _Ready()
    {
        Toggled += toggle;
        order.Pressed += toggleOrder;

        previousButton = buttons.GetNode<Button>("Alphabetical");

        foreach (Button button in buttons.GetChildren())
        {
            button.Pressed += () => selectSort(button);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            Rect2 rect = new(panel.GlobalPosition, panel.Size);
            Rect2 rect2 = new(GlobalPosition, Size);

            if (!rect.HasPoint(mouseButton.Position) && !rect2.HasPoint(mouseButton.Position))
            {
                ButtonPressed = false;
            }
        }
    }

    private void toggle(bool toggled)
    {
        panel.Visible = toggled;
    }

    private void selectSort(Button button)
    {
        if (Enum.TryParse<MapList.SortType>(button.Name, true, out var result) && MapList.Instance.Sorting.Value != result)
        {
            previousButton?.Disabled = false;
            button.Disabled = true;
            previousButton = button;

            MapList.Instance.Sorting.Value = result;
        }
    }

    private void toggleOrder()
    {
        MapList.Instance.Ascending.Value = !MapList.Instance.Ascending.Value;
        orderLabel.Text = MapList.Instance.Ascending.Value ? "Ascending" : "Descending";
    }
}
