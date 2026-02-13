using Godot;
using System;
using System.IO;

public partial class WorkerAnt : CharacterBody2D
{
	Vector2 mod = new Vector2(300, 0);
	Vector2[] path;
	int aim = 0;
	int counter = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Velocity -= new Vector2(0, -10);
		if (path == null && IsOnFloor())
		{
			counter ++;
			if (counter > 1)
			{
				path = Ant.pathFind(Position, Position + mod);
				GD.Print(path[0]);
				GD.Print(path[path.Length-1]);
			}
		}
		else if (path != null)
		{
			if (aim < path.Length && IsOnFloor())
			{
				float difference = path[aim].X - Position.X;
				if (compare(Position, path[aim]))
				{
					aim += 1;
				}
				else if (path[aim].Y <= Position.Y)
				{
					Velocity += new Vector2(difference/10, 0);
				}
				else
				{
					GD.Print(path[aim]);
					GD.Print(Position);

					GD.Print(1);

					MarchingSquares.edit(path[aim], true);
				}
				
			}
			else if (aim >= path.Length)
			{
				Velocity = new Vector2(0, Velocity.Y);
			}
			if (Velocity.X > 20)
			{
				Velocity = new Vector2(20, Velocity.Y);
			}
			else if (Velocity.X < -20)
			{
				Velocity = new Vector2(-20, Velocity.Y);
			}
		}
		//GD.Print(Velocity);
		MoveAndSlide();
	}

	private bool compare(Vector2 position, Vector2 aim)
	{
		if (position.X < aim.X && position.X + 15 > aim.X)
		{
			return true;
		}
		else if (position.X > aim.X && position.X - 15 < aim.X)
		{
			return true;
		}
		else if (position.X == aim.X)
		{
			return true;
		}
		return false;
	}
}
