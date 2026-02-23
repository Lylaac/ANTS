using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

public partial class WorkerAnt : CharacterBody2D
{
	RayCast2D backFloorDetector;
	RayCast2D frontFloorDetector;
	RayCast2D middleFloorDetector;
	Vector2 mod = new Vector2(1000, 0);
	Vector2 targetPosition;
	Vector2[] path;
	List<Vector2> marchingSquaresOld;
	(Vector2[], bool[]) pathFinding;
	bool[] digVectors;
	int aim = 0;
	int counter = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		backFloorDetector = GetNode("BackFloorDetector") as RayCast2D;
		frontFloorDetector = GetNode("FrontFloorDetector") as RayCast2D;
		middleFloorDetector = GetNode("MiddleFloorDetector") as RayCast2D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Velocity -= new Vector2(0, -10);

		if (path == null && IsOnFloor())
		{
				targetPosition = Position + mod;
				pathFinding = Ant.pathFind(Position, targetPosition);
				marchingSquaresOld = MarchingSquares.verts;
				path = pathFinding.Item1;
				digVectors = pathFinding.Item2;
				GD.Print(path[0]);
				GD.Print(path[path.Length-1]);
				
		}

		if (marchingSquaresOld != MarchingSquares.verts && IsOnFloor())
		{
			pathFinding = Ant.pathFind(Position, targetPosition);
			
			path = pathFinding.Item1;
			digVectors = pathFinding.Item2;
			marchingSquaresOld = MarchingSquares.verts;
			
		}
		if (path != null)
		{
			if (aim < path.Length && IsOnFloor())
			{
				float difference = path[aim].X - Position.X;
				if (compare(Position, path[aim]))
				{
					aim += 1;
				}
				else
				{
					Velocity += new Vector2(difference, 0);
				}
				if (aim+1 < digVectors.Length)
				{
					if(digVectors[aim+1])
					{
						MarchingSquares.edit(path[aim+1], true);
						MarchingSquares.edited = true;
					}
				}
				if (digVectors[aim])
				{
					MarchingSquares.edit(path[aim], true);
					MarchingSquares.edited = true;
				}
			}
			else if (aim >= path.Length)
			{
				Velocity = new Vector2(0, Velocity.Y);
				if (counter == 0)
				{
					GD.Print("done");
					counter = 1;
				}
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
		if(IsOnFloor())
		{
			float targetAngle = 0;
			
			if (backFloorDetector.IsColliding() && frontFloorDetector.IsColliding() && middleFloorDetector.IsColliding())
			{
				targetAngle = (Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal())) / 3;
			}
			else if (backFloorDetector.IsColliding() && frontFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (middleFloorDetector.IsColliding() && frontFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (middleFloorDetector.IsColliding() && backFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (backFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal());
			}
			else if (middleFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal());
			}
			else if (frontFloorDetector.IsColliding())
			{
				targetAngle = Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal());
			}
			
			//targetAngle = (Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal())) / 3;
			float currentAngle = Rotation;
			float differenceAngle = (currentAngle - targetAngle) * 10;
			Rotate(-differenceAngle * (float)delta);
			Velocity = Velocity.Rotated(-differenceAngle * (float)delta);
		}
		MoveAndSlide();
	}

	private bool compare(Vector2 position, Vector2 aim) // compares 2 vectors, if they are close enough (it will never be perfectly equal due to levels of precision in godot vectors) then we return true.
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
