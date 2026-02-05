using Godot;
using System;

public partial class StaticBody2d : StaticBody2D
{
	CollisionShape3D collisionShape2D = new CollisionShape3D();
	MeshInstance2D meshInstance2D = new MeshInstance2D();
	MarchingSquares marchingSquares = new MarchingSquares();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		generateMesh();
		AddChild(meshInstance2D);
		AddChild(collisionShape2D);
	}

    
	private void generateMesh()
    {
        var immMesh = new ImmediateMesh();
        Color col = new Color(255, 0, 0);

        immMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        immMesh.SurfaceSetColor(col);
        for (int count = 0; count < marchingSquares.verts.Count; count++)
        {
            immMesh.SurfaceSetUV(marchingSquares.uvs[count]);
            immMesh.SurfaceAddVertex2D(marchingSquares.verts[count]);
        }
        immMesh.SurfaceEnd();
        meshInstance2D.SetMesh(immMesh);
        var shape = immMesh.CreateTrimeshShape();
        var test = marchingSquares.verts.ToArray();
        collisionShape2D.SetShape(shape);
        
    }
    
    

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
