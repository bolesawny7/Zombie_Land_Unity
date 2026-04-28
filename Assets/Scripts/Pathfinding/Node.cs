using UnityEngine;

namespace ZombieLand.Pathfinding
{
    /// <summary>
    /// A single cell in the pathfinding grid. Stores its world position,
    /// whether it is walkable, and the A* search costs used while pathfinding.
    /// </summary>
    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public int gCost;     // distance from start
        public int hCost;     // heuristic distance to target
        public Node parent;   // back-pointer used to reconstruct the path

        public int FCost => gCost + hCost;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }
}
