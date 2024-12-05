using Godot;
using System;

public partial class IdeScene : Control
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
		if (@event.IsActionPressed("build_and_run"))
		{
			throw new InvalidCastException("It works!");
		}
    }
}
