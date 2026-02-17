using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public static class MarchingSquares
{
    // set the amount of columns of points we want and the amount of rows
    static int width = 50;
    static int height = 50;

    //Using the current millisecond as the seed to ensure different results each time
    static Random rnd = new Random(1);
    //DateTime.Now.Millisecond

    // Here we set a random height for the floor that is between 5% less than and 5% more than the center
    public static int floorHeight = rnd.Next((int)(height / 2 - (height * 0.05)), (int)(height / 2 + (height * 0.05)));

    // initialise the points array for interpolation
    static int[,] points = new int[height,width];

    // verts and uvs are used by the mesh to figure out where to draw corners and in what order to connect them
    public static List<Vector2> verts {get; set;}
    public static List<Vector2> uvs {get; set;}
    public static List<Vector2> edgeVerts {get; set;} // this is used to keep track of any vertexes that make up the perimeter of the shape, so that they can be used for the collision

    // this gets the resolution of the screen and then finds out what to multiply our numbers by to stretch the mesh to fill the screen
    static Vector2I screenSize = DisplayServer.WindowGetSize();
    public static float heightMult = (float)screenSize[1] / (height-1);
    static float widthMult = (float)screenSize[0] / (width-1);

    // this is for the groundMesh to check if there have been any edits to the mesh since it last updated
    public static bool edited {get; set;}



    // This function generates the values of all of the points that will be used for interpolation
    public static void generateField()
    {
        edited = false;
        //initialise our lists
        verts = [];
        uvs = [];
        edgeVerts = [];
        // for every point, if it is above the height of the floor, then set it to 10 (the max value)
        // if it is the floor height, set it between 9 and 0, showing a distance to a border between ground and air
        // if it is one below the height of the floor, set it between -1 and -9, showing it is underground and close to a border
        // if it is any more than one below the height of the floor, set it to -10, showing no distance to a border, and underground
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y < floorHeight)
                {
                    points[y,x] = 10;
                }
                else if (y == floorHeight)
                {
                    points[y,x] = rnd.Next(0,10);
                }
                else if (y == floorHeight+1)
                {
                    points[y,x] = rnd.Next(-9,0);
                }
                else
                {
                    points[y,x] = -10;
                }
            }
        }
    }

    // this generates the actual vertex list which we then feed into the mesh
    public static void generateVertexes()
    {
        
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // each square has 4 of our points as "corners"
                int topLeft = points[y,x];          int topRight = points[y,x+1];
                int bottomLeft = points[y+1,x];     int bottomRight = points[y+1,x+1];

                // this is an array of the values of each of the points
                int[] values = {topLeft, topRight, bottomLeft, bottomRight};

                // this denotes where each of the corners are on the screen
                float[,] cornerValues = {
                    {x * widthMult, y * heightMult}, 
                    {(x + 1) * widthMult, y * heightMult}, 
                    {x * widthMult, (y + 1) * heightMult}, 
                    {(x + 1) * widthMult, (y + 1) * heightMult}
                    };

                // these denote if each corner is underground or overground
                bool topLeftUnder = topLeft < 0;        bool topRightUnder = topRight < 0;
                bool bottomLeftUnder = bottomLeft < 0;  bool bottomRightUnder = bottomRight < 0;

                bool[] underPointsBools = [topLeftUnder, topRightUnder, bottomLeftUnder, bottomRightUnder];

                // this counts how many points are below the ground
                int underPointCount = underPointsBools.Count(s => s == true);

                // this is the list of vertexes that we will feed into our shape drawing functions
                List<float[]> vertexes = new List<float[]>();

                if (underPointCount == 4) // if all of the points are below the ground, then it must just be a solid square so we draw a quad between all four courners
                {
                    vertexes.Add([cornerValues[0,0], cornerValues[0,1]]);
                    vertexes.Add([cornerValues[1,0], cornerValues[1,1]]);
                    vertexes.Add([cornerValues[2,0], cornerValues[2,1]]);
                    vertexes.Add([cornerValues[3,0], cornerValues[3,1]]);
                    drawQuad(vertexes);
                }
                else if (underPointCount == 3) // if 3 of the points are under the ground, then we need 3 triangles, one between the 3 corners, and then a quad consisting of 2 triangles that makes up the extra area from the interpolation 
                {
                    
                    for (int i = 0; i < underPointsBools.Count(); i++)
                    {
                        if (underPointsBools[i])
                        {
                            vertexes.Add([cornerValues[i, 0], cornerValues[i, 1]]);
                        }
                    }

                    if (underPointsBools.SequenceEqual(new bool[]{true, true, true, false})) // bottom right false
                    {
                        vertexes.Add([cornerValues[1, 0], cornerValues[1,1]+heightMult*findShift(topRight, bottomRight)]);
                        vertexes.Add([cornerValues[2, 0]+widthMult*findShift(bottomLeft, bottomRight), cornerValues[2,1]]);

                        edgeVerts.Add(new Vector2(cornerValues[1, 0], cornerValues[1,1]+heightMult*findShift(topRight, bottomRight)));
                        edgeVerts.Add(new Vector2(cornerValues[2, 0]+widthMult*findShift(bottomLeft, bottomRight), cornerValues[2,1]));
                    }
                    else if (underPointsBools.SequenceEqual(new bool[]{true, true, false, true})) // bottom left false
                    {
                        vertexes.Add([cornerValues[0, 0], cornerValues[0,1]+heightMult*findShift(topLeft, bottomLeft)]);
                        vertexes.Add([cornerValues[3, 0]-widthMult*findShift(bottomRight, bottomLeft), cornerValues[3,1]]);

                        edgeVerts.Add(new Vector2(cornerValues[0, 0], cornerValues[0,1]+heightMult*findShift(topLeft, bottomLeft)));
                        edgeVerts.Add(new Vector2(cornerValues[3, 0]-widthMult*findShift(bottomRight, bottomLeft), cornerValues[3,1]));

                        (vertexes[1], vertexes[0]) = (vertexes[0], vertexes[1]);
                    }
                    else if (underPointsBools.SequenceEqual(new bool[]{true, false, true, true})) // top right false
                    {
                        vertexes.Add([cornerValues[0, 0]+widthMult*findShift(topLeft, topRight), cornerValues[0,1]]);
                        vertexes.Add([cornerValues[3, 0], cornerValues[3,1]-heightMult*findShift(bottomRight, topRight)]);

                        edgeVerts.Add(new Vector2(cornerValues[0, 0]+widthMult*findShift(topLeft, topRight), cornerValues[0,1]));
                        edgeVerts.Add(new Vector2(cornerValues[3, 0], cornerValues[3,1]-heightMult*findShift(bottomRight, topRight)));

                        (vertexes[1], vertexes[0]) = (vertexes[0], vertexes[1]);
                    }
                    else if (underPointsBools.SequenceEqual(new bool[]{false, true, true, true})) // top left false
                    {
                        vertexes.Add([cornerValues[2, 0], cornerValues[2,1]-heightMult*findShift(bottomLeft, topLeft)]);
                        vertexes.Add([cornerValues[1, 0]-widthMult*findShift(topRight, topLeft), cornerValues[1,1]]);

                        edgeVerts.Add(new Vector2(cornerValues[2, 0], cornerValues[2,1]-heightMult*findShift(bottomLeft, topLeft)));
                        edgeVerts.Add(new Vector2(cornerValues[1, 0]-widthMult*findShift(topRight, topLeft), cornerValues[1,1]));

                        (vertexes[2], vertexes[0]) = (vertexes[0], vertexes[2]);
                    }
                    drawPent(vertexes);
                }
                
                else if (underPointCount == 2) // if 2 points are under the ground, then there are a couple cases, 4 where they are adjacent corners, and 2 where they are opposing corners
                {
                    // add the active vertexes to the vertex list
                    for (int i = 0; i < underPointsBools.Count(); i++)
                    {
                        if (underPointsBools[i])
                        {
                            vertexes.Add([cornerValues[i, 0], cornerValues[i, 1]]);
                            
                        }
                    }

                    // if the top left and top right vertexes are underground
                    if (underPointsBools.SequenceEqual(new bool[]{true, true, false, false}))
                    {
                        sameCorners(vertexes, findShift(topLeft, bottomLeft), findShift(topRight, bottomRight), false);
                    }

                    // if the top left and bottom left vertexes are underground
                    else if (underPointsBools.SequenceEqual(new bool[]{true, false, true, false}))
                    {
                        sameCorners(vertexes, findShift(topLeft, topRight), findShift(bottomLeft, bottomRight), false);
                    }

                    // if the top left and bottom right corners are underground
                    else if (underPointsBools.SequenceEqual(new bool[]{true, false, false, true}))
                    {
                        oppositeCorners(vertexes, findShift(topLeft, topRight), findShift(topLeft, bottomLeft), findShift(bottomRight, bottomLeft), findShift(bottomRight, topRight), true);
                    }

                    // if the top right and bottom left corners are underground
                    else if (underPointsBools.SequenceEqual(new bool[]{false, true, true, false}))
                    {
                        oppositeCorners(vertexes, findShift(topRight, topLeft), findShift(topRight, bottomRight), findShift(bottomLeft, bottomRight), findShift(bottomLeft, topLeft), true);
                    }

                    // if the top right and bottom right corners are underground
                    else if (underPointsBools.SequenceEqual(new bool[]{false, true, false, true}))
                    {
                        sameCorners(vertexes, findShift(topRight, topLeft), findShift(bottomRight, bottomLeft), true);
                    }

                    //if the bottom left and bottom right corners are underground
                    else if (underPointsBools.SequenceEqual(new bool[]{false, false, true, true}))
                    {
                        sameCorners(vertexes, findShift(bottomLeft, topLeft), findShift(bottomRight, topRight), true);
                    }
                }
                else if (underPointCount == 1) // if only one point is below ground then we need to draw a single triangle between the underground corner and the 2 interpolated values.
                {
                    if (underPointsBools[0])
                    {
                        vertexes.Add([cornerValues[0,0], cornerValues[0,1]]);
                        vertexes.Add([findShift(topLeft, topRight)*widthMult+cornerValues[0,0], cornerValues[0,1]]);
                        vertexes.Add([cornerValues[0,0], findShift(topLeft, bottomLeft)*heightMult+cornerValues[0,1]]);

                        edgeVerts.Add(new Vector2(findShift(topLeft, topRight)*widthMult+cornerValues[0,0], cornerValues[0,1]));
                        edgeVerts.Add(new Vector2(cornerValues[0,0], findShift(topLeft, bottomLeft)*heightMult+cornerValues[0,1]));
                    }
                    else if (underPointsBools[1])
                    {
                        vertexes.Add([cornerValues[1,0], cornerValues[1,1]]);
                        vertexes.Add([-findShift(topRight, topLeft)*widthMult+cornerValues[1,0], cornerValues[1,1]]);
                        vertexes.Add([cornerValues[1,0], findShift(topRight, bottomRight)*heightMult+cornerValues[1,1]]);

                        edgeVerts.Add(new Vector2(-findShift(topRight, topLeft)*widthMult+cornerValues[1,0], cornerValues[1,1]));
                        edgeVerts.Add(new Vector2(cornerValues[1,0], findShift(topRight, bottomRight)*heightMult+cornerValues[1,1]));
                    }
                    else if (underPointsBools[2])
                    {
                        vertexes.Add([cornerValues[2,0], cornerValues[2,1]]);
                        vertexes.Add([findShift(bottomLeft, bottomRight)*widthMult+cornerValues[2,0], cornerValues[2,1]]);
                        vertexes.Add([cornerValues[2,0], -findShift(bottomLeft, topLeft)*heightMult+cornerValues[2,1]]);

                        edgeVerts.Add(new Vector2(findShift(bottomLeft, bottomRight)*widthMult+cornerValues[2,0], cornerValues[2,1]));
                        edgeVerts.Add(new Vector2(cornerValues[2,0], -findShift(bottomLeft, topLeft)*heightMult+cornerValues[2,1]));                        
                    }
                    else
                    {
                        vertexes.Add([cornerValues[3,0], cornerValues[3,1]]);
                        vertexes.Add([-findShift(bottomRight, bottomLeft)*widthMult+cornerValues[3,0], cornerValues[3,1]]);
                        vertexes.Add([cornerValues[3,0], -findShift(bottomRight, topRight)*heightMult+cornerValues[3,1]]);

                        edgeVerts.Add(new Vector2(-findShift(bottomRight, bottomLeft)*widthMult+cornerValues[3,0], cornerValues[3,1]));
                        edgeVerts.Add(new Vector2(cornerValues[3,0], -findShift(bottomRight, topRight)*heightMult+cornerValues[3,1]));
                    }
                    drawTriangle(vertexes);
                    
                }
                
                
            }
        }
        
        edgeVerts.Add(new Vector2(width*widthMult, height*heightMult)); // add the bottom left and bottom right corners to the array, we do bottom right first because they need to be in order
        edgeVerts.Add(new Vector2(0, height*heightMult));


    }
    
    private static void drawQuad(List<float[]> vertexes)
    {
        // First Triangle
        drawTriangle([vertexes[0], vertexes[1], vertexes[2]]);

        //Second Triangle
        drawTriangle([vertexes[1], vertexes[2], vertexes[3]]);
    }
    private static void drawTriangle(List<float[]> vertexes)
    {
        // add the vertexes to the verts list and the uvs (which track what order stuff should be connected) to their list, counting up
        uvs.Add(new Vector2(uvs.Count, 0));
        verts.Add(new Vector2(vertexes[0][0], vertexes[0][1]));
        uvs.Add(new Vector2(uvs.Count, 0));
        verts.Add(new Vector2(vertexes[1][0], vertexes[1][1]));
        uvs.Add(new Vector2(uvs.Count, 0));
        verts.Add(new Vector2(vertexes[2][0], vertexes[2][1]));
    }
    private static void oppositeCorners(List<float[]> vertexes, float horiShift1, float vertShift1, float horiShift2, float vertShift2, bool topRight) // this is run when 2 corners are underground but not adjacent
    {
        // we have shift one and shift two, these are used one for each corner to interpolate, but we also need horizontal and vertical versions as the bottom vertexes will need to go up
        if (topRight)
        {
            horiShift1 *= -1;
        }
        else
        {
            horiShift2 *= -1;
        }
        // make a list for the top corner's vertexes and the bottom's
        List<float[]> vertexesTop = [
            [vertexes[0][0], vertexes[0][1]],
            [vertexes[0][0], vertexes[0][1]+vertShift1*heightMult],
            [vertexes[0][0]+horiShift1*widthMult, vertexes[0][1]]
        ];
        List<float[]> vertexesBottom = [
            [vertexes[1][0], vertexes[1][1]],
            [vertexes[1][0], vertexes[1][1]-vertShift2*heightMult],
            [vertexes[1][0]+horiShift2*widthMult, vertexes[1][1]]
        ];
        drawTriangle(vertexesTop);
        drawTriangle(vertexesBottom);

        edgeVerts.Add(new Vector2(vertexes[0][0], vertexes[0][1]+vertShift1*heightMult));
        edgeVerts.Add(new Vector2(vertexes[0][0]+horiShift1*widthMult, vertexes[0][1]));
        edgeVerts.Add(new Vector2(vertexes[1][0], vertexes[1][1]-vertShift2*heightMult));
        edgeVerts.Add(new Vector2(vertexes[1][0]+horiShift2*widthMult, vertexes[1][1]));
    }
    private static void sameCorners(List<float[]> vertexes, float shift1, float shift2, bool bottomOrRightSide) // this is run when 2 corners are underground and are adjacent
    {
        // the first shift is for one side, the second shift is for the second side, this is because it is likely the 2 points will have different values, which means we need to apply different interpolation

        // this is a flag to say if it is a right side or a bottom side, then we need our 2 interpolated vertexes above/to the right of the original 2, which requires a negative shift
        if (bottomOrRightSide)
        {
            shift1 *= -1;
            shift2 *= -1;
        }
        if (vertexes[0][0] == vertexes[1][0]) // if the x values are the same, then it is a right or left side, otherwise it must be a top or bottom side, therefore we apply our shifts in different places
        {
            vertexes.Add([(float)(vertexes[0][0]+shift1*widthMult), vertexes[0][1]]);
            vertexes.Add([(float)(vertexes[1][0]+shift2*widthMult), vertexes[1][1]]);

            edgeVerts.Add(new Vector2((float)(vertexes[0][0]+shift1*widthMult), vertexes[0][1]));
            edgeVerts.Add(new Vector2((float)(vertexes[1][0]+shift2*widthMult), vertexes[1][1]));
        }
        else
        {
            vertexes.Add([vertexes[0][0], (float)(vertexes[0][1]+shift1*heightMult)]);
            vertexes.Add([vertexes[1][0], (float)(vertexes[1][1]+shift2*heightMult)]);

            edgeVerts.Add(new Vector2(vertexes[0][0], (float)(vertexes[0][1]+shift1*heightMult)));
            edgeVerts.Add(new Vector2(vertexes[1][0], (float)(vertexes[1][1]+shift2*heightMult)));
        }
        drawQuad(vertexes);
    }
    private static void drawPent(List<float[]> vertexes)
    {
        drawTriangle([vertexes[0], vertexes[1], vertexes[2]]);
        drawTriangle([vertexes[1], vertexes[2], vertexes[3]]);
        drawTriangle([vertexes[2], vertexes[3], vertexes[4]]);
    }
    private static float findShift(float a, float b)
    {
        return (-a)/(b-a);
    }
    public static void edit(Vector2 editPos, bool subtract)
    {
        edited = true;
        edgeVerts.Clear();
        int modifier = -1;
        if (subtract)
        {
            modifier = 1;
        }
        int x = (int)Math.Round(Math.Clamp(editPos[0], 0, screenSize[0]) / widthMult);
        int y = (int)Math.Round(Math.Clamp(editPos[1], 0, screenSize[1]) / heightMult);

        

        verts = new List<Vector2>();
        uvs = new List<Vector2>();
        points[y,x] = Math.Clamp(points[y,x] + modifier, -10, 10);
        points[y,x-1] = Math.Clamp(points[y,x-1] + modifier, -10, 10);
        points[y,x+1] = Math.Clamp(points[y,x+1] + modifier, -10, 10);

        generateVertexes();
    }
}