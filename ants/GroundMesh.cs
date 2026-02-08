using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public partial class GroundMesh : MeshInstance2D
{
    MarchingSquares marchingSquares = new MarchingSquares();
    StaticBody2D staticBody2D = new StaticBody2D();
	ConcavePolygonShape2D shape = new ConcavePolygonShape2D();
	CollisionShape2D collisionShape = new CollisionShape2D();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        generateMesh();
        collisionShape.SetShape(shape);
		staticBody2D.AddChild(collisionShape);
        AddChild(staticBody2D);

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

    private void generateMesh()
    {
        var immMesh = new ImmediateMesh();
        Color col = new Color(255, 0, 0);
        
        immMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        immMesh.SurfaceSetColor(col);
        for (int count = 0; count < marchingSquares.verts.Count; count++)
        {
            immMesh.SurfaceAddVertex2D(marchingSquares.verts[count]);
        }
        immMesh.SurfaceEnd();
        SetMesh(immMesh);
        Godot.Vector2[] vertices = marchingSquares.edgeVerts.ToArray();
        shape.SetSegments(vertices);
    }

    public override void _Input(InputEvent @event)
    {
        var mousePos = GetViewport().GetMousePosition();
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    marchingSquares.edit(mousePos, true);
                    generateMesh();
                    break;
                case MouseButton.Right:
                    marchingSquares.edit(mousePos, false);
                    generateMesh();
                    break;
            }
        }
    }
}
