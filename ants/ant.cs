using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public static class Ant
{
    
    public static Vector2[] pathFind(Vector2 start, Vector2 end)
    {
        Vector2 center = start;
        int currentDistance = 0;
        List<Vector2> travelVectors = [];
        travelVectors.Add(start);
        int vectorGap = 20;
        while (center.X != end.X || center.Y!= end.Y)
        {
            Vector2[] surroundingCoordinates = {
            center with { X = center.X - vectorGap, Y = center.Y - vectorGap }, center with { X = center.X, Y = center.Y - vectorGap },  center with { X = center.X + vectorGap, Y = center.Y - vectorGap },
            center with { X = center.X - vectorGap, Y = center.Y },                                                                      center with { X = center.X + vectorGap, Y = center.Y },
            center with { X = center.X - vectorGap, Y = center.Y + vectorGap }, center with { X = center.X, Y = center.Y + vectorGap },  center with { X = center.X + vectorGap, Y = center.Y + vectorGap }
            };
            double lowestDistance = 1000;
            int bestVector = 0;
            for (int count = 0; count <  surroundingCoordinates.Length; count ++)
            {
                double distancesAddHeuristics = currentDistance + heuristicFind(surroundingCoordinates[count], end);
                
                if (Geometry2D.IsPointInPolygon(surroundingCoordinates[count], MarchingSquares.edgeVerts.ToArray()))
                {
                    distancesAddHeuristics += 1;
                }
                
                if (distancesAddHeuristics < lowestDistance)
                {
                    lowestDistance = distancesAddHeuristics;
                    bestVector = count;
                }
            }
            travelVectors.Add(surroundingCoordinates[bestVector]);
            center = surroundingCoordinates[bestVector];
            currentDistance += 1;
        }
        return travelVectors.ToArray();
        

    }
    
    public static double heuristicFind(Vector2 coordinates, Vector2 destination)
    {
        float xDis = coordinates.X - destination.X;
        float yDis = coordinates.Y - destination.Y;
        return Math.Sqrt(Math.Pow(xDis, 2) + Math.Pow(yDis, 2));
    }
}