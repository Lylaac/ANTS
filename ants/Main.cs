using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	Random rng = new Random(DateTime.Now.Millisecond);
	PackedScene WorkerAnt = GD.Load<PackedScene>("res://workerAnt.tscn");
	static List<WorkerAnt> ants = [];
	int startAntCount = 3;
	int random = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int floorHeight = (int)Math.Round(MarchingSquares.floorHeight * MarchingSquares.heightMult);
		for (int antCount = 0; antCount < startAntCount; antCount++)
		{
			WorkerAnt instance = WorkerAnt.Instantiate() as WorkerAnt;
			instance.uniqueID = antCount;
			ants.Add(instance);
			instance.SetPosition(new Vector2(600 + antCount*300, floorHeight-50));
			AddChild(instance);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void antRandomiser() // this is called every 5 seconds by a timer node
	{
		foreach(WorkerAnt ant in ants) // for every ant that exists
		{
			for (int loopCount = 0; loopCount < 3; loopCount++) // for each need
			{
				random = rng.Next(0, 100); // generate a random number between 1 and 100
				if (random <= 5) // if the number is less than 5, giving us a 5 percent chance
				{
					switch (loopCount) // depending on where in the loop we are, we will subtract one from one of the 3 needs
					{
						case 0:
							ant.food --;
							break;
						case 1:
							ant.water --;
							break;
						case 2:
							ant.sleep --;
							break;
					}
				}
			}
		}
	}
}
