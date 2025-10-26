using System.Collections.Generic;
using UnityEngine;

public class BFS : MonoBehaviour
{
    private static readonly Vector2Int[] directions = {
        new (1, 0),   // phải
        new (-1, 0),  // trái
        new (0, 1),   // lên
        new (0, -1)   // xuống
    };

    public class Node
    {
        public Vector2Int pos;
        public Vector2Int dir;
        public int turn;

        public Node(Vector2Int pos, Vector2Int dir, int turn)
        {
            this.pos = pos;
            this.dir = dir;
            this.turn = turn;
        }

        // Dùng để so sánh trong HashSet / Dictionary
        public override bool Equals(object obj)
        {
            if (obj is not Node other) return false;
            return pos == other.pos && dir == other.dir;
        }

        public override int GetHashCode()
        {
            return pos.GetHashCode() ^ dir.GetHashCode();
        }
    }

    public static List<Vector2Int> FindPath(Tile[,] tiles, Vector2Int start, Vector2Int end)
    {
        Queue<Node> queue = new();
        HashSet<Node> visited = new();
        Dictionary<Node, Node> parent = new();

        // Bắt đầu từ start, hướng (0,0), turn = 0
        Node startNode = new(start, Vector2Int.zero, 0);
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            // Nếu đến đích
            if (current.pos == end)
            {
                List<Vector2Int> path = new();
                Node step = current;
                while (parent.ContainsKey(step))
                {
                    path.Add(step.pos);
                    step = parent[step];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            // Duyệt 4 hướng
            foreach (Vector2Int dir in directions)
            {
                Vector2Int nextPos = current.pos + dir;
                int newTurn = current.dir == Vector2Int.zero || dir == current.dir ? current.turn : current.turn + 1;

                Node nextNode = new(nextPos, dir, newTurn);

                if (IsValid(nextNode, tiles, visited, end))
                {
                    // Không cho phép rẽ quá 2 lần
                    if (nextNode.turn > 2) continue;

                    queue.Enqueue(nextNode);
                    visited.Add(nextNode);
                    parent[nextNode] = current;
                }
            }
        }

        return null; // không có đường hợp lệ
    }

    private static bool IsValid(Node node, Tile[,] tiles, HashSet<Node> visited, Vector2Int end)
    {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        // Giới hạn trong map
        if (node.pos.x < 0 || node.pos.x >= width || node.pos.y < 0 || node.pos.y >= height)
            return false;

        // Nếu đã thăm node này với cùng hướng
        if (visited.Contains(node))
            return false;

        // Cho phép đi vào ô end
        if (node.pos == end)
            return true;

        // Ô bị chiếm
        if (tiles[node.pos.x, node.pos.y].Occupied)
            return false;

        return true;
    }
}
