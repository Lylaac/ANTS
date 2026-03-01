using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{
	Random rng = new Random(DateTime.Now.Millisecond);
	PackedScene WorkerAnt = GD.Load<PackedScene>("res://workerAnt.tscn");
	static List<WorkerAnt> ants = [];
	int startAntCount = 1;
	int random = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GroundMesh test = GetNode("GroundMesh") as GroundMesh;
		int floorHeight = (int)Math.Round(MarchingSquares.floorHeight * MarchingSquares.heightMult);
		for (int antCount = 0; antCount < startAntCount; antCount++)
		{
			WorkerAnt instance = WorkerAnt.Instantiate() as WorkerAnt;
			
			ants.Add(instance);
			instance.SetPosition(new Vector2(1200 + antCount*300, floorHeight-50));
			AddChild(instance);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void antRandomiser()
	{
		GD.Print(2);
		foreach(WorkerAnt ant in ants)
		{
			random = rng.Next(0, 100);
			ant.fiveSecondTimer();
			for (int loopCount = 0; loopCount < 3; loopCount++)
			{
				random = rng.Next(0, 100);
				if (random <= 5)
				{
					switch (loopCount)
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
