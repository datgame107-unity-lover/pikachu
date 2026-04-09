using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Breadth-First Search pathfinder for the Pikachu/Pokémon connect puzzle.
/// Finds a valid connecting path between two cells with a maximum of 2 direction changes.
/// </summary>
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

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to find a connecting path between <paramref name="start"/> and
    /// <paramref name="end"/> on the given tile grid.
    /// </summary>
    /// <returns>
    /// An ordered list of grid positions forming the path, or <c>null</c> if no
    /// valid path exists within the turn limit.
    /// </returns>
    public static List<Vector2Int> FindPath(Tile[,] tiles, Vector2Int start, Vector2Int end)
    {
        if (tiles == null)
        {
            Debug.LogWarning("[BFS] Tile grid is null.");
            return null;
        }

        var queue = new Queue<PathNode>();
        var visited = new HashSet<PathNode>();
        var parent = new Dictionary<PathNode, PathNode>();

        var startNode = new PathNode(start, Vector2Int.zero, turns: 0);
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            PathNode current = queue.Dequeue();

            if (current.Position == end)
                return ReconstructPath(parent, current, start);

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int nextPos = current.Position + dir;
                int newTurns = (current.Direction == Vector2Int.zero || dir == current.Direction)
                    ? current.Turns
                    : current.Turns + 1;

                if (newTurns > MaxTurns) continue;

                var nextNode = new PathNode(nextPos, dir, newTurns);

                if (!IsTraversable(nextNode, tiles, visited, end)) continue;

                visited.Add(nextNode);
                parent[nextNode] = current;
                queue.Enqueue(nextNode);
            }
        }

        return null; // No valid path found
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

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

        // Out of bounds
        if (node.Position.x < 0 || node.Position.x >= width ||
            node.Position.y < 0 || node.Position.y >= height)
            return false;

        // Already visited with same state
        if (visited.Contains(node)) return false;

        // Destination cell is always reachable
        if (node.Position == end) return true;

        // Blocked by an occupied tile
        if (tiles[node.Position.x, node.Position.y].IsOccupied) return false;

        return true;
    }

    // -------------------------------------------------------------------------
    // Inner types
    // -------------------------------------------------------------------------

    /// <summary>
    /// Represents a search state: the current grid position, the direction we
    /// arrived from, and how many direction changes have occurred so far.
    /// </summary>
    private sealed class PathNode
    {
        public readonly Vector2Int Position;
        public readonly Vector2Int Direction;
        public readonly int Turns;

        public PathNode(Vector2Int position, Vector2Int direction, int turns)
        {
            Position = position;
            Direction = direction;
            Turns = turns;
        }

        public override bool Equals(object obj) =>
            obj is PathNode other &&
            Position == other.Position &&
            Direction == other.Direction;

        public override int GetHashCode() =>
            Position.GetHashCode() ^ (Direction.GetHashCode() << 2);
    }
}