using System;
using System.Collections.Generic;
using Godot;

public partial class WorkerAnt : CharacterBody2D
{
	RayCast2D backFloorDetector;
	RayCast2D frontFloorDetector;
	RayCast2D middleFloorDetector;
	RayCast2D frontDetector1;
	RayCast2D frontDetector2;
	RayCast2D frontDetector3;
	Vector2 mod = new Vector2(1000, -100);
	List<Vector2> path;
	int aim = 0;
	public int food = 10;
	public int water = 10;
	public int sleep = 10;
	float xDifference;
	float yDifference;
	bool flipped = false;
	float targetAngle;
	float currentAngle;
	float differenceAngle;
	bool buildingMainCorridor = false;
	bool underground = false;
	public int uniqueID;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		backFloorDetector = GetNode("BackFloorDetector") as RayCast2D;
		frontFloorDetector = GetNode("FrontFloorDetector") as RayCast2D;
		middleFloorDetector = GetNode("MiddleFloorDetector") as RayCast2D;
		frontDetector1 = GetNode("FrontDetector1") as RayCast2D;
		frontDetector2 = GetNode("FrontDetector2") as RayCast2D;
		frontDetector3 = GetNode("FrontDetector3") as RayCast2D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		underground = Geometry2D.IsPointInPolygon(Position, MarchingSquares.undergroundBoundaries.ToArray());
		if (!underground)
		{
			Velocity += new Vector2(0, 10);
		}
		if (path == null)
		{
			if (food < water && food < sleep && food < 5)
			{
				
			}
			else if (water < food && water < sleep && water < 5)
			{
				
			}
			else if (sleep < food && sleep < water && sleep < 5)
			{
				
			}
			else
			{
				idleChecks();
			}
		}
		if (path != null)
		{
			if (aim < path.Count && (IsOnFloor() || underground))
			{
				if (path[aim].Y > Position.Y && IsOnFloor())
				{
					MarchingSquares.edit(Position, true);
					
					MarchingSquares.edited = true;
				}
				yDifference = path[aim].Y - Position.Y;
				if (!underground)
				{
					yDifference = 0;
				}
				xDifference = path[aim].X - Position.X;

				if (xDifference < -5 && !flipped)
				{
					flip();	
				}
				else if (xDifference > 5 && flipped)
				{
					flip();
				}
				if (metPoint(path[aim]))
				{
					
					aim += 1;
				}
				else
				{
					
					Velocity += new Vector2(xDifference, yDifference);
				}
				

				if (buildingMainCorridor)
				{
					if (Position.Y >= 150 + MarchingSquares.floorHeight *MarchingSquares.heightMult && Ant.foodRoomStart == Vector2.Zero)
					{
						Ant.foodRoomStart = Position;
					}
					else if (Position.Y >= 300 + MarchingSquares.floorHeight *MarchingSquares.heightMult && Ant.sleepRoomStart == Vector2.Zero)
					{
						Ant.sleepRoomStart = Position;
					}
					else if (Position.Y >= 450 + MarchingSquares.floorHeight *MarchingSquares.heightMult && Ant.throneRoomStart == Vector2.Zero)
					{
						Ant.throneRoomStart = Position;
					}
				}
				
			}
			else if (aim >= path.Count)
			{
				path = null;
				aim = 0;
				Velocity = new Vector2(0, Velocity.Y);
				if (buildingMainCorridor)
				{
					buildingMainCorridor = false;
				}
			}

			if (frontDetector1.IsColliding())
			{
				MarchingSquares.edit(frontDetector1.TargetPosition + frontDetector1.GlobalPosition, true);
				MarchingSquares.edited = true;
			}
			if (frontDetector2.IsColliding())
			{
				MarchingSquares.edit(frontDetector2.TargetPosition + frontDetector2.GlobalPosition, true);
				MarchingSquares.edited = true;
			}
			if (frontDetector3.IsColliding())
			{
				MarchingSquares.edit(frontDetector3.TargetPosition + frontDetector3.GlobalPosition, true);
				MarchingSquares.edited = true;
			}
			if (Velocity.X > 20)
			{
				Velocity = new Vector2(20, Velocity.Y);
			}
			else if (Velocity.X < -20)
			{
				Velocity = new Vector2(-20, Velocity.Y);
			}
			
			if (Velocity.Y < -20)
			{
				Velocity = new Vector2(Velocity.X, -20);
			}
		}
		
		if(backFloorDetector.IsColliding() || frontFloorDetector.IsColliding() || middleFloorDetector.IsColliding())
		{
			
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
			
			currentAngle = Rotation;
			differenceAngle = (currentAngle - targetAngle) * 10;
			
			Rotate(-differenceAngle * (float)delta);
			
			Velocity = Velocity.Rotated(-differenceAngle * (float)delta);
			
		}
		else if (path != null)
		{
			if (aim < path.Count)
			{
				LookAt(path[aim]);
				Rotate((float)(Math.PI/2));
			}
		}
		
		MoveAndSlide();
	}

	private bool metPoint(Vector2 aim) 
	{
		
		bool returner = true;
		if (Position.Y > aim.Y && Velocity.Y < 0)
		{
			returner = false;
		}
		else if (Position.Y < aim.Y && Velocity.Y > 0)
		{
			returner = false;
		}
		else if (Position.X > aim.X && Velocity.X < 0)
		{
			returner = false;
		}
		else if (Position.X < aim.X && Velocity.X > 0)
		{
			returner = false;
		}
		else if (Velocity.X == 0 || Velocity.Y == 0)
		{
			returner = false;
		}
		return returner;
	}

	private void flip() // this function flips every single child of the node from left to right or back again
	{
		Vector2 invert = new Vector2(-1, 1);
		Sprite2D sprite = GetNode("Sprite2D") as Sprite2D;
		sprite.FlipH = !sprite.FlipH;

		backFloorDetector.Position *= invert;
		middleFloorDetector.Position *= invert;
		frontFloorDetector.Position *= invert;

		frontDetector1.TargetPosition *= invert;
		frontDetector1.Position *= invert;

		frontDetector2.TargetPosition *= invert;
		frontDetector2.Position *= invert;

		frontDetector3.TargetPosition *= invert;
		frontDetector3.Position *= invert;
		

		CollisionPolygon2D charBody = GetNode("CollisionPolygon2D") as CollisionPolygon2D;
		charBody.SetScale(new Vector2(-charBody.Scale.X, charBody.Scale.Y));

		flipped = !flipped;
	}

	private void idleChecks()
	{
		// check if all the important rooms exist and if they don't, build them
		if (Ant.mainCorridorStart == Vector2.Zero && (Ant.mainCorridorBuilder == uniqueID || Ant.mainCorridorBuilder == -1))
		{
			Ant.mainCorridorBuilder = uniqueID;
			if (comparePositionX(DisplayServer.WindowGetSize()/2))
			{
				path = Ant.pathFind(Position, Position + new Vector2(500, 500), true);
				Ant.mainCorridorStart = Position;
				buildingMainCorridor = true;
				Ant.mainCorridorBuilder = -2;
			}
			else
			{
				path = Ant.pathFind(Position, new Vector2(DisplayServer.WindowGetSize().X/2, Position.Y), false);
			}
		}
		else if (Ant.foodRoomStart != Vector2.Zero && (Ant.foodRoomBuilder == uniqueID || Ant.foodRoomBuilder == -1))
		{
			Ant.foodRoomBuilder = uniqueID;
			Vector2 offset = new Vector2(Ant.foodRoomStart.X - 30, Ant.foodRoomStart.Y);
			if (comparePositionXY(Ant.foodRoomStart))
			{
				path = Ant.pathFind(Position, offset, true);
				Ant.foodRoomBuilder = -2;
			}
			else
			{
				path = Ant.pathFind(Position, Ant.foodRoomStart, false);
			}
		}
		else if (Ant.sleepRoomStart != Vector2.Zero && (Ant.sleepRoomBuilder == uniqueID || Ant.sleepRoomBuilder == -1))
		{
			Ant.sleepRoomBuilder = uniqueID;
			Vector2 offset = new Vector2(Ant.sleepRoomStart.X - 30, Ant.sleepRoomStart.Y);
			if (comparePositionXY(Ant.sleepRoomStart))
			{
				path = Ant.pathFind(Position, offset, true);
				Ant.sleepRoomBuilder = -2;
			}
			else
			{
				path = Ant.pathFind(Position, Ant.sleepRoomStart, false);
			}
		}
		else if (Ant.throneRoomStart != Vector2.Zero && (Ant.throneRoomBuilder == uniqueID || Ant.throneRoomBuilder == -1))
		{
			Ant.foodRoomBuilder = uniqueID;
			Vector2 offset = new Vector2(Ant.foodRoomStart.X - 30, Ant.foodRoomStart.Y);
			if (comparePositionXY(Ant.foodRoomStart))
			{
				path = Ant.pathFind(Position, offset, true);
				Ant.throneRoomBuilder = -2;
			}
			else
			{
				path = Ant.pathFind(Position, Ant.foodRoomStart, false);
			}
		}
		
	}
	public bool comparePositionXY(Vector2 point)
	{
		if (Math.Abs(Position.X - point.X) <= 5 && Math.Abs(Position.Y - point.Y) <= 5)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	private bool comparePositionX(Vector2 point)
	{
		if (Math.Abs(Position.X - point.X) <= 5)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
