using Godot;

public partial class GroundMesh : MeshInstance2D
{
    StaticBody2D staticBody2D = new StaticBody2D();
	ConcavePolygonShape2D shape = new ConcavePolygonShape2D();
	CollisionShape2D collisionShape = new CollisionShape2D();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        MarchingSquares.generateField();
        MarchingSquares.generateVertexes();
        generateMesh();
        collisionShape.SetShape(shape);
		staticBody2D.AddChild(collisionShape);
        AddChild(staticBody2D);

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (MarchingSquares.edited)
        {
            generateMesh();
            MarchingSquares.edited = false;
        }
    }

    private void generateMesh()
    {
        var immMesh = new ImmediateMesh();
        Color col = new Color(255, 0, 0);
        
        immMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        immMesh.SurfaceSetColor(col);
        for (int count = 0; count < MarchingSquares.verts.Count; count++)
        {
            immMesh.SurfaceAddVertex2D(MarchingSquares.verts[count]);
        }
        immMesh.SurfaceEnd();
        SetMesh(immMesh);
        Vector2[] vertices = MarchingSquares.edgeVerts.ToArray();
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
                    MarchingSquares.edit(mousePos, true);
                    generateMesh();
                    break;
                case MouseButton.Right:
                    MarchingSquares.edit(mousePos, false);
                    generateMesh();
                    break;
            }
        }
    }
}
