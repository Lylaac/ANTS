using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public static class Ant // an abstract class full of stuff we will need for the 3 different types of ant
{
    
    public static (Vector2[], bool[]) pathFind(Vector2 start, Vector2 end) // An A star pathfinding algorithm.
    {
        start = new Vector2(roundToNearestTen(start.X), roundToNearestTen(start.Y));
        end = new Vector2(roundToNearestTen(end.X), roundToNearestTen(end.Y));
        Vector2 center = start;
        int currentDistance = 0;
        List<Vector2> visitedList = [];
        List<Vector2> travelVectors = []; // this is our actual path we will be returning
        List<bool> digVectors = []; // the vectors we need to dig on
        int vectorGap = 10; // this is the gap between vectors, a bigger gap means less vectors scanned but also lower accuracy, lower gap is laggier
        while (center.X != end.X || center.Y!= end.Y) // while we haven't found our end node
        {
            // create a list of the surrounding vectors
            Vector2[] surroundingCoordinates = {
            center with { X = center.X - vectorGap, Y = center.Y },                                                                      center with { X = center.X + vectorGap, Y = center.Y },
            center with { X = center.X - vectorGap, Y = center.Y + vectorGap }, center with { X = center.X, Y = center.Y + vectorGap },  center with { X = center.X + vectorGap, Y = center.Y + vectorGap },
            center with { X = center.X - vectorGap, Y = center.Y - vectorGap },                                                          center with { X = center.X + vectorGap, Y = center.Y - vectorGap }
            };
            double lowestDistance = Int32.MaxValue; // set this to an impossibly high number so it will always get overwritten
            int bestVector = 0; // keeps track of what is the next closest vector to travel to in order to get to the end vector
            for (int count = 0; count <  surroundingCoordinates.Length; count ++)
            {
                double distancesAddHeuristics = currentDistance + heuristicFind(surroundingCoordinates[count], end); // add the current distance plus our heuristic
                
                if (Geometry2D.IsPointInPolygon(surroundingCoordinates[count], MarchingSquares.edgeVerts.ToArray())) // add a penalty for nodes under the ground, digging is slower unless you have to take a massive detour or cannot do it otherwise
                {
                    distancesAddHeuristics += 0.1;
                }
                
                if (distancesAddHeuristics < lowestDistance && !visitedList.Contains(surroundingCoordinates[count])) // lowest distance + heuristic wins
                {
                    lowestDistance = distancesAddHeuristics;
                    bestVector = count;
                }
            }
            if (Geometry2D.IsPointInPolygon(surroundingCoordinates[bestVector], MarchingSquares.edgeVerts.ToArray()))
            {
                digVectors.Add(true);
            }
            else
            {
                digVectors.Add(false);
            }
            visitedList.Add(surroundingCoordinates[bestVector]);
            travelVectors.Add(surroundingCoordinates[bestVector]); // add the vector to the path, change the center so this is our new vector, then increase the distance by one and do it all over again!
            center = surroundingCoordinates[bestVector];
            currentDistance += 1;
        }
        return (travelVectors.ToArray(), digVectors.ToArray());
        

    }
    
    public static double heuristicFind(Vector2 coordinates, Vector2 destination) // this calculates a heuristic for each vector to estimate how close it is to the end node (we are just finding the straight line distance with pythagoras)
    {
        float xDis = coordinates.X - destination.X;
        float yDis = coordinates.Y - destination.Y;
        return Math.Sqrt(Math.Pow(xDis, 2) + Math.Pow(yDis, 2));
    }
    private static float roundToNearestTen(double value)
    {
        value = Math.Round(value);
        string strVal = value.ToString();
        if (strVal[strVal.Length-1] < 5)
        {
            return 10*((int)value/10);
        }
        else
        {
            return 10*(((int)value+1) / 10);
        }
    }
}