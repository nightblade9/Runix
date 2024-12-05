using Godot;
using System;
using Runix;

public partial class IdeScene : Control
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
		if (!@event.IsActionPressed("build_and_run"))
		{
			return;
		}

		var buildStatusLabel = GetNode("VBoxContainer/BuildStatus") as Label;
		
		var node = GetNode("VBoxContainer/Code");
		var textEdit = node as TextEdit;
		var code = textEdit.Text;
		
		buildStatusLabel.Text = "Compiling ...";
		var isSuccess = Runix.Compiler.Compile(code);
		buildStatusLabel.Text = $"Build {(isSuccess ? "succeeded" : "failed!")}";

		if (!isSuccess)
		{
			var resultsLabel = GetNode("VBoxContainer/CompilerResults") as Label;
			resultsLabel.Text = String.Join("\n", Runix.Compiler.s_LastFailures);
			return;
		}

		var result = Runix.Compiler.Run();
		Godot.GD.Print($"Result: {result?.ToString()}");
		// TODO: nah, not here.
		buildStatusLabel.Text = $"Result: {result?.ToString()}";
    }
}
