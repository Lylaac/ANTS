using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public static class Ant // an abstract class full of stuff we will need for the 3 different types of ant
{
    // these vectors keep track of important places that the ants need to "remember"
    public static Vector2 foodRoomStart = Vector2.Zero;
    public static Vector2 sleepRoomStart = Vector2.Zero;
    public static Vector2 throneRoomStart = Vector2.Zero;
    public static Vector2 mainCorridorStart = Vector2.Zero;

    // these keep track of which ant is doing what
    public static int foodRoomBuilder = -1;
    public static int sleepRoomBuilder = -1;
    public static int throneRoomBuilder = -1;
    public static int mainCorridorBuilder = -1;


    public static List<Vector2> pathFind(Vector2 start, Vector2 end, bool ignoreDiggingCost) // An A star pathfinding algorithm.
    {
        start = new Vector2(roundToNearestTen(start.X), roundToNearestTen(start.Y));
        end = new Vector2(roundToNearestTen(end.X), roundToNearestTen(end.Y));
        Vector2 center = start;
        int currentDistance = 0;
        List<Vector2> visitedList = [];
        List<Vector2> travelVectors = []; // this is our actual path we will be returning
        int vectorGap = 5; // this is the gap between vectors, a bigger gap means less vectors scanned but also lower accuracy, lower gap is laggier
        while (center.X != end.X || center.Y!= end.Y) // while we haven't found our end node
        {
            // create a list of the surrounding vectors
            Vector2[] surroundingCoordinates = {
            center with { X = center.X - vectorGap, Y = center.Y },                                                                      center with { X = center.X + vectorGap, Y = center.Y },
            center with { X = center.X - vectorGap, Y = center.Y + vectorGap }, center with { X = center.X, Y = center.Y + vectorGap },  center with { X = center.X + vectorGap, Y = center.Y + vectorGap },
            center with { X = center.X - vectorGap, Y = center.Y - vectorGap }, center with { X = center.X, Y = center.Y - vectorGap },  center with { X = center.X + vectorGap, Y = center.Y - vectorGap }
            };
            double lowestDistance = int.MaxValue; // set this to an impossibly high number so it will always get overwritten
            int bestVector = 0; // keeps track of what is the next closest vector to travel to in order to get to the end vector
            for (int count = 0; count <  surroundingCoordinates.Length; count ++)
            {
                double distancesAddHeuristics = currentDistance + heuristicFind(surroundingCoordinates[count], end); // add the current distance plus our heuristic
                
                if (Geometry2D.IsPointInPolygon(surroundingCoordinates[count], MarchingSquares.edgeVerts.ToArray()) && !ignoreDiggingCost) // add a penalty for nodes under the ground, digging is slower unless you have to take a massive detour or cannot do it otherwise, ignoring digging costs if we are told to do so, for tasks that require digging
                {
                    distancesAddHeuristics += 100;
                }
                
                if (distancesAddHeuristics < lowestDistance && !visitedList.Contains(surroundingCoordinates[count])) // lowest distance + heuristic wins
                {
                    lowestDistance = distancesAddHeuristics;
                    bestVector = count;
                }
            }
            visitedList.Add(surroundingCoordinates[bestVector]);
            travelVectors.Add(surroundingCoordinates[bestVector]); // add the vector to the path, change the center so this is our new vector, then increase the distance by one and do it all over again!
            center = surroundingCoordinates[bestVector];
            currentDistance += 1;
        }
        return travelVectors;
        

    }
    
    public static double heuristicFind(Vector2 coordinates, Vector2 destination) // this calculates a heuristic for each vector to estimate how close it is to the end node (we are just finding the straight line distance with pythagoras)
    {
        float xDis = coordinates.X - destination.X;
        float yDis = coordinates.Y - destination.Y;
        return Math.Sqrt(Math.Pow(xDis, 2) + Math.Pow(yDis, 2));
    }
    private static float roundToNearestTen(double value) // this, as stated, rounds the given value to the nearest 10
    {
        value = Math.Round(value); // round it to a full number first
        string strVal = value.ToString(); //convert to a string then if the last item in the string (the number in the "ones" column) is under 5 then it is round down
        if (strVal[strVal.Length-1] < 5) 
        {
            return 10*((int)value/10); // rounds down using integer division
        }
        else
        {
            return 10*(((int)value+1) / 10); //rounds up using integer division
        }
    }
}