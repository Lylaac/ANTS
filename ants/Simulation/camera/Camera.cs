using Godot;
using System;

public partial class Camera : Camera2D
{
	Vector2I screenSize = DisplayServer.WindowGetSize();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * (new Vector2(10,10)/Zoom);
		GlobalPosition += inputDirection;
		if (Input.IsActionJustPressed("zoom_in"))
		{
			GlobalPosition = GetGlobalMousePosition();
			Zoom += new Vector2(1, 1);

		}
		else if (Input.IsActionJustPressed("zoom_out") && Zoom - new Vector2(1,1) != new Vector2(0,0))
		{
			
			Zoom -= new Vector2(1,1);
		}
	}
}
