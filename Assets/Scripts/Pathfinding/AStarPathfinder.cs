using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Pathfinding
{
    /// <summary>
    /// Classic A* search over the PathfindingGrid. Returns a list of world
    /// positions to walk through, or null if no path exists.
    /// Uses an octile distance heuristic (10 straight, 14 diagonal).
    /// </summary>
    public static class AStarPathfinder
    {
        public static List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
        {
            var grid = PathfindingGrid.Instance;
            if (grid == null) return null;

            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node endNode = grid.NodeFromWorldPoint(endPos);

            if (startNode == null || endNode == null) return null;
            if (!startNode.walkable) startNode = FindNearestWalkable(startNode, grid);
            if (!endNode.walkable) endNode = FindNearestWalkable(endNode, grid);
            if (startNode == null || endNode == null) return null;

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = GetDistance(startNode, endNode);
            startNode.parent = null;
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node current = openSet[0];
                int currentIndex = 0;
                for (int i = 1; i < openSet.Count; i++)
                {
                    Node candidate = openSet[i];
                    if (candidate.FCost < current.FCost ||
                        (candidate.FCost == current.FCost && candidate.hCost < current.hCost))
                    {
                        current = candidate;
                        currentIndex = i;
                    }
                }

                openSet.RemoveAt(currentIndex);
                closedSet.Add(current);

                if (current == endNode)
                    return RetracePath(startNode, endNode);

                foreach (Node neighbour in grid.GetNeighbours(current))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                    int newCost = current.gCost + GetDistance(current, neighbour);
                    bool inOpen = openSet.Contains(neighbour);

                    if (newCost < neighbour.gCost || !inOpen)
                    {
                        neighbour.gCost = newCost;
                        neighbour.hCost = GetDistance(neighbour, endNode);
                        neighbour.parent = current;
                        if (!inOpen) openSet.Add(neighbour);
                    }
                }
            }

            return null;
        }

        static List<Vector3> RetracePath(Node start, Node end)
        {
            var path = new List<Vector3>();
            Node current = end;
            while (current != start && current != null)
            {
                path.Add(current.worldPosition);
                current = current.parent;
            }
            path.Reverse();
            return path;
        }

        // Octile distance: efficient and admissible for 8-directional grids.
        static int GetDistance(Node a, Node b)
        {
            int dx = Mathf.Abs(a.gridX - b.gridX);
            int dy = Mathf.Abs(a.gridY - b.gridY);
            return dx > dy ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
        }

        // If the requested node is blocked (e.g. zombie spawned slightly inside a wall),
        // we walk outward in a small spiral to find a usable starting cell.
        static Node FindNearestWalkable(Node node, PathfindingGrid grid)
        {
            for (int radius = 1; radius <= 4; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;
                        Vector3 sample = node.worldPosition + new Vector3(dx, 0, dy) * (grid.nodeRadius * 2f);
                        Node candidate = grid.NodeFromWorldPoint(sample);
                        if (candidate.walkable) return candidate;
                    }
                }
            }
            return null;
        }
    }
}
