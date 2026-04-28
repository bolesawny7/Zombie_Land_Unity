using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Pathfinding
{
    /// <summary>
    /// Builds a 2D grid of Nodes covering an XZ rectangle in the world.
    /// Each node is marked unwalkable when a collider on `obstacleMask`
    /// overlaps it. The grid is used by AStarPathfinder.
    /// </summary>
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        public Vector2 gridWorldSize = new Vector2(40f, 40f);
        public float nodeRadius = 0.5f;
        public LayerMask obstacleMask;

        Node[,] grid;
        float nodeDiameter;
        int gridSizeX, gridSizeY;

        void Awake()
        {
            Instance = this;
            nodeDiameter = nodeRadius * 2f;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        }

        /// <summary>
        /// Scans the world and builds the walkable/unwalkable grid.
        /// Call after walls have been spawned (and Physics.SyncTransforms called).
        /// </summary>
        public void BuildGrid()
        {
            grid = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position
                - Vector3.right * gridWorldSize.x * 0.5f
                - Vector3.forward * gridWorldSize.y * 0.5f;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft
                        + Vector3.right * (x * nodeDiameter + nodeRadius)
                        + Vector3.forward * (y * nodeDiameter + nodeRadius);

                    bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius * 0.9f, obstacleMask);
                    grid[x, y] = new Node(walkable, worldPoint, x, y);
                }
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x * 0.5f) / gridWorldSize.x;
            float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y * 0.5f) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentX), 0, gridSizeX - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentY), 0, gridSizeY - 1);
            return grid[x, y];
        }

        public List<Node> GetNeighbours(Node node)
        {
            var neighbours = new List<Node>(8);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = node.gridX + dx;
                    int ny = node.gridY + dy;
                    if (nx < 0 || nx >= gridSizeX || ny < 0 || ny >= gridSizeY) continue;

                    // Avoid cutting corners through diagonal walls.
                    if (dx != 0 && dy != 0)
                    {
                        if (!grid[node.gridX + dx, node.gridY].walkable) continue;
                        if (!grid[node.gridX, node.gridY + dy].walkable) continue;
                    }

                    neighbours.Add(grid[nx, ny]);
                }
            }
            return neighbours;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0.1f, gridWorldSize.y));

            if (grid == null) return;
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 0f, 0f, 0.4f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter * 0.9f));
            }
        }
    }
}
