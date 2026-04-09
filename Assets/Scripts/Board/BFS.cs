using NUnit.Framework;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class BFS
{
    private static readonly Vector2Int[] Directions =

    {

        new( 1,  0), // Right

        new(-1,  0), // Left

        new( 0,  1), // Up

        new( 0, -1), // Down

    };

    private const int MaxTurns = 2;

    public static List<Vector2Int> FindPath(Tile[,] tiles, Vector2Int start, Vector2Int end)
    {
        if (tiles == null) return null;

        var Queue = new Queue<PathNode>();
        HashSet<PathNode> visited = new HashSet<PathNode>();
        Dictionary<PathNode, PathNode> parent = new Dictionary<PathNode, PathNode>();

        PathNode startNode = new PathNode(start, Vector2Int.zero, 0);
        Queue.Enqueue(startNode);
        visited.Add(startNode);

        while (Queue.Count > 0)
        {
            PathNode current = Queue.Dequeue();
            if (current.Position == end)
            {
                return ReconstructPath(parent, current, start);


            }

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int nextPos = current.Position + dir;
                int newTurns = (current.Direction == dir ? current.Turns : current.Turns + 1);

                if (newTurns > MaxTurns) continue;
                var nextNode = new PathNode(nextPos, dir, newTurns);
                if (!IsTraversable(nextNode, tiles, visited, end))
                    continue;

                visited.Add(nextNode);
                parent[nextNode] = current;
                Queue.Enqueue(nextNode);

            }
        }
        return null;
    }

    private static List<Vector2Int> ReconstructPath(

        Dictionary<PathNode, PathNode> parent,

        PathNode endNode,

        Vector2Int start)

    {

        var path = new List<Vector2Int>();

        PathNode step = endNode;



        while (parent.ContainsKey(step))

        {

            path.Add(step.Position);

            step = parent[step];

        }



        path.Add(start);

        path.Reverse();

        return path;

    }
    private static bool IsTraversable(

    PathNode node,

    Tile[,] tiles,

    HashSet<PathNode> visited,

    Vector2Int end)

    {

        int width = tiles.GetLength(0);

        int height = tiles.GetLength(1);




        if (node.Position.x < 0 || node.Position.x >= width ||

            node.Position.y < 0 || node.Position.y >= height)

            return false;




        if (visited.Contains(node)) return false;


        if (node.Position == end) return true;


        if (tiles[node.Position.x, node.Position.y].IsOccupied) return false;



        return true;

    }
}


public sealed class PathNode
{
    public readonly Vector2Int Position;
    public readonly Vector2Int Direction;
    public readonly int Turns;

    public PathNode(Vector2Int Position, Vector2Int Direction, int Turns)
    {
        this.Position = Position;
        this.Direction = Direction;
        this.Turns = Turns;
    }

}
