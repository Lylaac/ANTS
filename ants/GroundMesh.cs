using Godot;
using System;
using System.Collections.Generic;

public partial class GroundMesh : MeshInstance2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        List<Vector3> verts = [];
        List<Vector2> uvs = [];

		MarchingSquares marchingSquares = new MarchingSquares();

        verts = marchingSquares.verts;
		uvs = marchingSquares.uvs;

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();

        var arrMesh = Mesh as ArrayMesh;
        if (arrMesh != null)
        {
            // Create mesh surface from mesh array
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
		}
		SetMesh(arrMesh);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
