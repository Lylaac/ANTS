using System;
using System.Collections.Generic;
using Godot;

public partial class WorkerAnt : CharacterBody2D
{
	RayCast2D backFloorDetector; //explained on line 31
	RayCast2D frontFloorDetector;
	RayCast2D middleFloorDetector;
	RayCast2D frontDetector1; //explained on line 36
	RayCast2D frontDetector2;
	RayCast2D frontDetector3;
	List<Vector2> path; // the list of different vectors we need to travel to in order to go to where we want to go
	int aim = 0; // keeps track of what index in the path we are aiming to go
	float xDifference; // the difference between where we are aiming to go and the place we currently are on the x axis
	float yDifference; // same as above but for the y axis instead
	bool flipped = false; // keeps track of if we are facing left or right, if it is false we are facing right, if it is true we are facing left
	float targetAngle; // these are used in the calculations of the rotation, target angle is the bearing of where we want to face from directly up
	float currentAngle; // current angle is the angle we are currently facing
	float differenceAngle; // difference angle is a fraction of the distance between target and current angle, which we turn every frame allowing for smooth rotation
	bool buildingMainCorridor = false; // keeps track of if we are building the main corridor of the farm as special cases apply, such as assigning where the other rooms connect
	bool underground = false; // explained on line 45
	public int uniqueID; // unique id for each ant so that we can take different tasks and keep track of who's doing what
	public int food = 10; // these are our needs, they slowly go down over time
	public int water = 10;
	public int sleep = 10; 
	bool selected = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// assign all of our raycasts that are used for detecting the floor for rotational purposes
		backFloorDetector = GetNode("BackFloorDetector") as RayCast2D;
		frontFloorDetector = GetNode("FrontFloorDetector") as RayCast2D;
		middleFloorDetector = GetNode("MiddleFloorDetector") as RayCast2D;
		// assign all of the raycasts that are used for detecting stuff in front of us that we may need to dig away
		frontDetector1 = GetNode("FrontDetector1") as RayCast2D;
		frontDetector2 = GetNode("FrontDetector2") as RayCast2D;
		frontDetector3 = GetNode("FrontDetector3") as RayCast2D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ant_select")) // if alt and leftclick are pressed
		{
			if (!selected && comparePositionXY(GetGlobalMousePosition(), 20)) // if we aren't selected and are clicked on, then become selected
			{
				selected = true;
			}
			else if (selected) // if we are selected, navigate to where we want to go and then become unselected
			{
				path = Ant.pathFind(Position, GetGlobalMousePosition(), true);
				selected = false;
			}
		}
		underground = Geometry2D.IsPointInPolygon(Position, MarchingSquares.undergroundBoundaries.ToArray()); // check if we are "underground" or below the surface, as this means we have a back wall to hold onto for climbing
		if (!underground) // if we aren't underground then there's nothing to climb on so we do fall
		{
			Velocity += new Vector2(0, 10); // this just applies gravity
		}
		if (path == null)
		{
				idleChecks(); // we aren't doing anything then do the idle checks
		}
		if (path != null) // if we have somewhere to go
		{
			if (aim < path.Count && (IsOnFloor() || underground)) // if the aim is less than the length of the path and if we are either on the floor or underground, no movement in the air so no flying
			{
				if (path[aim].Y > Position.Y && IsOnFloor()) // if we need to go down and the floor is below us, dig down
				{
					MarchingSquares.edit(Position, true);
					
					MarchingSquares.edited = true;
				}

				yDifference = path[aim].Y - Position.Y; // explained on line 16

				if (!underground) // if we aren't underground, we cannot modify our own y velocity
				{
					yDifference = 0;
				}

				xDifference = path[aim].X - Position.X; //explained line 15

				if (xDifference < -5 && !flipped) // if we need to go left and we are not flipped, flip us
				{
					flip();	
				}
				else if (xDifference > 5 && flipped) // if we want to go right and we are flipped, flip us
				{
					flip();
				}
				if (metPoint(path[aim])) // if we are where we want to be then increment the aim counter
				{
					
					aim += 1;
				}
				else //otherwise keep modifying our velocity
				{
					
					Velocity += new Vector2(xDifference, yDifference);
				}
				

				if (buildingMainCorridor) // if we are building the main corridor then we need the different rooms to be designated at different heights
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
			else if (aim >= path.Count) // if the aim is more than the path we have finished the path, stop moving
			{
				path = null;
				aim = 0;
				Velocity = new Vector2(0, Velocity.Y);
				if (buildingMainCorridor) // if we were building the main corridor, we have just finished and so need to turn off this bool
				{
					buildingMainCorridor = false;
				}
			}

			// for each of our 3 raycasts facing out the front, if they are colliding we need to dig 
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

			// cap both our horizontal and vertical velocities
			if (Velocity.X > 20)
			{
				Velocity = new Vector2(20, Velocity.Y);
			}
			else if (Velocity.X < -20)
			{
				Velocity = new Vector2(-20, Velocity.Y);
			}
			
			if (Velocity.Y < -20) // Vertical is only cappped upwards, as gravity would otherwise be really slow
			{
				Velocity = new Vector2(Velocity.X, -20);
			}
		}
		
		// if any of our floor detection raycasts are colliding then we need to run some code to determine the average angle of the floor, this is the target angle
		if(backFloorDetector.IsColliding() || frontFloorDetector.IsColliding() || middleFloorDetector.IsColliding())
		{
			//each of these if statements are simply for different combinations of colliders colliding, meaning we need to take different average
			if (backFloorDetector.IsColliding() && frontFloorDetector.IsColliding() && middleFloorDetector.IsColliding()) // if all three are colliding
			{
				targetAngle = (Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal())) / 3;
			}
			else if (backFloorDetector.IsColliding() && frontFloorDetector.IsColliding()) // if only the back and front are colliding
			{
				targetAngle = Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (middleFloorDetector.IsColliding() && frontFloorDetector.IsColliding()) // if only the middle and the front are colliding
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (middleFloorDetector.IsColliding() && backFloorDetector.IsColliding()) // if only the middle and the back are colliding
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal()) + Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal()) / 2;
			}
			else if (backFloorDetector.IsColliding()) // if only the back is colliding
			{
				targetAngle = Vector2.Up.AngleTo(backFloorDetector.GetCollisionNormal());
			}
			else if (middleFloorDetector.IsColliding()) // if only the middle is colliding
			{
				targetAngle = Vector2.Up.AngleTo(middleFloorDetector.GetCollisionNormal());
			}
			else if (frontFloorDetector.IsColliding()) // if only the front is colliding
			{
				targetAngle = Vector2.Up.AngleTo(frontFloorDetector.GetCollisionNormal());
			}
			
			// this was explained around lines 18-20
			currentAngle = Rotation;
			differenceAngle = (currentAngle - targetAngle) * 10;
			
			Rotate(-differenceAngle * (float)delta);
			
			Velocity = Velocity.Rotated(-differenceAngle * (float)delta);
			
		}
		else if (path != null) //if we aren't on the ground and are aiming somewhere, look at the point we are aiming at
		{
			if (aim < path.Count)
			{
				LookAt(path[aim]);
				Rotate((float)(Math.PI/2));
			}
		}
		
		MoveAndSlide();
	}

	private bool metPoint(Vector2 aim) // this checks if we have met or exceeded the point we are aiming for 
	{
		
		bool returner = true;
		if (Position.Y > aim.Y && Velocity.Y < 0) // if we are below the point and going up then we haven't met it
		{
			returner = false;
		}
		else if (Position.Y < aim.Y && Velocity.Y > 0) // if above and moving down then we haven't met or exceeded it
		{
			returner = false;
		}
		else if (Position.X > aim.X && Velocity.X < 0) // if we are right of the point and moving to the left we haven't met or exceeded it
		{
			returner = false;
		}
		else if (Position.X < aim.X && Velocity.X > 0) // if we are left of it and moving right then we haven't met or exceeded it
		{
			returner = false;
		}
		else if (Velocity.X == 0 || Velocity.Y == 0) // if we aren't moving in one axis it causes bugs with godots physics engine and this calculation
		{
			returner = false;
		}
		return returner;
	}

	private void flip() // this function flips every single child of the node from left to right or back again
	{
		Vector2 invert = new Vector2(-1, 1);
		Sprite2D sprite = GetNode("Sprite2D") as Sprite2D;
		sprite.FlipH = !sprite.FlipH; //sprites have an inbuilt flip so we can just call that

		//multiplying local position within the node by -1, 1 means the x gets flipped but not the y, this is like a reflection. this is usable for raycasts as they're symmetrical
		backFloorDetector.Position *= invert; 
		middleFloorDetector.Position *= invert;
		frontFloorDetector.Position *= invert;

		frontDetector1.TargetPosition *= invert;
		frontDetector1.Position *= invert;

		frontDetector2.TargetPosition *= invert;
		frontDetector2.Position *= invert;

		frontDetector3.TargetPosition *= invert;
		frontDetector3.Position *= invert;
		

		CollisionPolygon2D charBody = GetNode("CollisionPolygon2D") as CollisionPolygon2D; // the collision body has no inbuilt flip and is not symetrical, however we can modify the scale on the x axis to be negative what it is now and that gives the illusion of flipping it.
		charBody.SetScale(new Vector2(-charBody.Scale.X, charBody.Scale.Y));

		flipped = !flipped; // invert the flipped boolean
	}

	private void idleChecks()
	{
		// check if all the important rooms exist and if they don't, build them
		if (Ant.mainCorridorStart == Vector2.Zero && (Ant.mainCorridorBuilder == uniqueID || Ant.mainCorridorBuilder == -1)) // if the main corridor does not exist and has no builder or if this ant is the assigned builder
		{
			Ant.mainCorridorBuilder = uniqueID; // set the builder id of the main corridor to be our id
			if (comparePositionX(DisplayServer.WindowGetSize()/2, 5)) // if we are close enough to the center of the screen we can begin digging
			{
				path = Ant.pathFind(Position, Position + new Vector2(500, 500), true); // dig down in a 45 degree angle
				Ant.mainCorridorStart = Position; // start is where we are right now
				buildingMainCorridor = true; // we are building the main corridor so we set this to true
				Ant.mainCorridorBuilder = -2; // then we set the builder id to -2 as this no longer needs to be built by anyone
			}
			else // if we aren't close enough to the center, go there
			{
				path = Ant.pathFind(Position, new Vector2(DisplayServer.WindowGetSize().X/2, Position.Y), false); // if the food room does not exist and has no builder or if this ant is the assigned builder
			}
		}
		else if (Ant.foodRoomStart != Vector2.Zero && (Ant.foodRoomBuilder == uniqueID || Ant.foodRoomBuilder == -1))
		{
			Ant.foodRoomBuilder = uniqueID; // set the builder id of the food room to be our id
			Vector2 offset = new Vector2(Ant.foodRoomStart.X - 30, Ant.foodRoomStart.Y); // this is an offset of how far from the corridor we should go out to fully build the room
			if (comparePositionXY(Ant.foodRoomStart, 5)) // if we are at the start of the room
			{
				path = Ant.pathFind(Position, offset, true); // start to build it, ignoring the cost of digging
				Ant.foodRoomBuilder = -2; // set builder id to -2 as this no longer needs to be built
			}
			else
			{
				path = Ant.pathFind(Position, Ant.foodRoomStart, false); // otherwise just go to the start of the room
			}
		}
		else if (Ant.sleepRoomStart != Vector2.Zero && (Ant.sleepRoomBuilder == uniqueID || Ant.sleepRoomBuilder == -1)) // this is the exact same as the food room it just accesses the sleep room variables instead
		{
			Ant.sleepRoomBuilder = uniqueID;
			Vector2 offset = new Vector2(Ant.sleepRoomStart.X - 30, Ant.sleepRoomStart.Y);
			if (comparePositionXY(Ant.sleepRoomStart, 5))
			{
				path = Ant.pathFind(Position, offset, true);
				Ant.sleepRoomBuilder = -2;
			}
			else
			{
				path = Ant.pathFind(Position, Ant.sleepRoomStart, false);
			}
		}
		else if (Ant.throneRoomStart != Vector2.Zero && (Ant.throneRoomBuilder == uniqueID || Ant.throneRoomBuilder == -1)) // and again, same as the food and sleep rooms, just for the throne room instead where the queen ant will reside
		{
			Ant.foodRoomBuilder = uniqueID;
			Vector2 offset = new Vector2(Ant.foodRoomStart.X - 30, Ant.foodRoomStart.Y);
			if (comparePositionXY(Ant.foodRoomStart, 5))
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
	public bool comparePositionXY(Vector2 point, int margin) //compares x and y distances to see if we are close enough to where we need to be (within 5 pixels)
	{
		if (Math.Abs(Position.X - point.X) <= margin && Math.Abs(Position.Y - point.Y) <= margin)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	private bool comparePositionX(Vector2 point, int margin) // compare only the x to see if we are in the right place horizontally, ignoring vertical placement
	{
		if (Math.Abs(Position.X - point.X) <= margin)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
