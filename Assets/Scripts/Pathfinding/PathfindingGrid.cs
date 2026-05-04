using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Pathfinding
{
    /// <summary>
    /// Builds a 2D grid of Nodes covering an XZ rectangle in the world.
    /// Each node is marked unwalkable when a collider on `obstacleMask`
    /// overlaps it. The grid is used by AStarPathfinder.
    ///
    /// IMPORTANT: <see cref="gridSizeX"/> and <see cref="gridSizeY"/> are
    /// (re)computed inside <see cref="BuildGrid"/> from the current
    /// <see cref="gridWorldSize"/> and <see cref="nodeRadius"/>. Doing this
    /// in BuildGrid (and not Awake) means callers can configure the grid
    /// after AddComponent without size/index mismatches — the previous bug
    /// where this method ran with stale defaults caused world->grid
    /// coordinate skew, which made every chasing zombie walk east instead
    /// of toward the player.
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
        }

        public void BuildGrid()
        {
            nodeDiameter = nodeRadius * 2f;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

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
            if (grid == null) return null;

            // Convert world → fractional grid coordinates using the
            // ACTUAL grid extent (gridSizeX * nodeDiameter), not
            // gridWorldSize, so the two never disagree.
            float halfWidth = gridSizeX * nodeDiameter * 0.5f;
            float halfDepth = gridSizeY * nodeDiameter * 0.5f;

            float localX = worldPosition.x - transform.position.x + halfWidth;
            float localZ = worldPosition.z - transform.position.z + halfDepth;

            int x = Mathf.Clamp(Mathf.FloorToInt(localX / nodeDiameter), 0, gridSizeX - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(localZ / nodeDiameter), 0, gridSizeY - 1);
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
            Gizmos.DrawWireCube(transform.position,
                new Vector3(gridWorldSize.x, 0.1f, gridWorldSize.y));

            if (grid == null) return;
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 0f, 0f, 0.4f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter * 0.9f));
            }
        }
    }
}
