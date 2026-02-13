using Godot;
using System;
using System.Linq;

public partial class Main : Node2D
{
	PackedScene WorkerAnt = GD.Load<PackedScene>("res://WorkerAnt.tscn");
	int startAntCount = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GroundMesh test = GetNode("GroundMesh") as GroundMesh;
		int floorHeight = (int)Math.Round(MarchingSquares.floorHeight * MarchingSquares.heightMult);
		GD.Print(floorHeight);
		for (int antCount = 0; antCount < startAntCount; antCount++)
		{
			CharacterBody2D instance = WorkerAnt.Instantiate() as CharacterBody2D;
			
			instance.SetPosition(new Vector2(100 + antCount*300, floorHeight-25));
			AddChild(instance);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
