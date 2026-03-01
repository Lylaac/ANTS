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
	Vector2 targetPosition;
	Vector2[] path;
	int aim = 0;
	int counter = 0;
	public int food = 10;
	public int water = 10;
	public int sleep = 10;
	float xDifference;
	float yDifference;
	bool flipped = false;
	bool test;
	float targetAngle;
	float currentAngle;
	float differenceAngle;
	bool buildingMainCorridor = false;

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
		Velocity -= new Vector2(0, -10);
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
			if (aim < path.Length && IsOnFloor())
			{
				if (path[aim].Y > Position.Y)
				{
					MarchingSquares.edit(path[aim], true);
					
					MarchingSquares.edited = true;
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
				if (compare(Position, path[aim]))
				{
					aim += 1;
				}
				else
				{
					Velocity += new Vector2(xDifference, 0);
				}
				
				
			}
			else if (aim >= path.Length)
			{
				path = null;
				aim = 0;
				Velocity = new Vector2(0, Velocity.Y);
				if (buildingMainCorridor)
				{
					buildingMainCorridor = false;
				}
				GD.Print(Ant.foodRoomStart);
				GD.Print(Ant.sleepRoomStart);
				GD.Print(Ant.throneRoomStart);
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
		}
		
		if(IsOnFloor())
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
		
		MoveAndSlide();
	}

	private bool compare(Vector2 position, Vector2 aim) // compares 2 vectors, if they are close enough (it will never be perfectly equal due to levels of precision in godot vectors) then we return true.
	{
		if (position.X > aim.X && Velocity.X > 0)
		{
			return true;
		}
		else if (position.X < aim.X && Velocity.X < 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void flip()
	{
		Sprite2D sprite = GetNode("Sprite2D") as Sprite2D;
		sprite.FlipH = !sprite.FlipH;

		backFloorDetector.Position *= new Vector2(-1, 1);
		middleFloorDetector.Position *= new Vector2(-1, 1);
		frontFloorDetector.Position *= new Vector2(-1, 1);

		frontDetector1.TargetPosition *= new Vector2(-1, 1);
		frontDetector1.Position *= new Vector2(-1, 1);

		frontDetector2.TargetPosition *= new Vector2(-1, 1);
		frontDetector2.Position *= new Vector2(-1, 1);

		frontDetector3.TargetPosition *= new Vector2(-1, 1);
		frontDetector3.Position *= new Vector2(-1, 1);
		

		CollisionPolygon2D charBody = GetNode("CollisionPolygon2D") as CollisionPolygon2D;
		charBody.SetScale(new Vector2(-charBody.Scale.X, charBody.Scale.Y));

		flipped = !flipped;
	}

	private void idleChecks()
	{
		if (Ant.mainCorridorStart == null)
		{
			if (Position.X - DisplayServer.WindowGetSize().X/2 < 5 && Position.X - DisplayServer.WindowGetSize().X/2 > -5)
			{
				path = Ant.pathFind(Position, Position + new Vector2(500, 500));
				Ant.mainCorridorStart = Position;
				buildingMainCorridor = true;
			}
			else
			{
				path = Ant.pathFind(Position, new Vector2(DisplayServer.WindowGetSize().X/2, Position.Y));
			}
		}
		else if (Ant.foodRoomStart == null)
		{
			
		}
		else if (Ant.sleepRoomStart == null)
		{
			
		}
		else if (Ant.throneRoomStart == null)
		{
			
		}
		
	}
	public void fiveSecondTimer()
	{
		GD.Print(1);
		if (buildingMainCorridor)
		{
			if (Ant.foodRoomStart == null)
			{
				Ant.foodRoomStart = Position;
			}
			else if (Ant.sleepRoomStart == null)
			{
				Ant.sleepRoomStart = Position;
			}
			else if (Ant.throneRoomStart == null)
			{
				Ant.throneRoomStart = Position;
			}
		}
	}
}
