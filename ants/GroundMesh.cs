using Godot;
using System;
using System.Collections.Generic;

public partial class GroundMesh : MeshInstance2D
{
    MarchingSquares marchingSquares = new MarchingSquares();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        generateMesh();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

    private void generateMesh()
    {
        var immMesh = new ImmediateMesh();
        immMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        for (int count = 0; count < marchingSquares.verts.Count; count++)
        {
            immMesh.SurfaceSetUV(marchingSquares.uvs[count]);
            immMesh.SurfaceAddVertex2D(marchingSquares.verts[count]);
        }
        immMesh.SurfaceEnd();
        SetMesh(immMesh);
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
                    break;
                case MouseButton.Right:
                    marchingSquares.edit(mousePos, false);
                    break;
            }
        }
        generateMesh();
    }
}
