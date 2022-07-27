using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for A* pathfinding.
/// </summary>
public class Pathfinding : MonoBehaviour
{
    public int diagonalMoveCost = 14;
    public int adjacentMoveCost = 10;

    [Header("Debug")]
    public bool debug;
    public Color regPathColor;
    public Color simPathColor;
    public Vector3[] regPath;
    public Vector3[] simPath;

    /// <summary>
    /// Starts a coroutine to find a path from start to target.
    /// </summary>
    /// <param name="start">The starting position</param>
    /// <param name="target">The target position</param>
    public void StartFindPath(Vector3 start, Vector3 target)
    {
        StartCoroutine(FindPath(start, target));
    }

    /// <summary>
    /// Find a path between the starting position and target position using
    /// the A* algorithm.
    /// </summary>
    /// <param name="_startPos">The starting position</param>
    /// <param name="_targetPos">The target position</param>
    private IEnumerator FindPath(Vector3 _startPos, Vector3 _targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        MapNode startNode = MapManager.Instance.GetMapNodeFromPosition(MapManager.Instance.WorldPositionToMapPosition(_startPos));
        MapNode targetNode = MapManager.Instance.GetMapNodeFromPosition(MapManager.Instance.WorldPositionToMapPosition(_targetPos));

        //Debug.Log($"{gameObject.name} | Start Pos: {startNode.mapPosition} | Target Pos: {targetNode.mapPosition}");

        MinHeap<MapNode> openSet = new MinHeap<MapNode>(MapManager.Instance.MapSize);
        HashSet<MapNode> closedSet = new HashSet<MapNode>();

        openSet.Push(startNode);

        while (openSet.Count > 0)
        {
            // Find the cheapest node to travel to first
            MapNode currentNode = openSet.Pop();
            closedSet.Add(currentNode);

            // If we reach the target
            //Debug.Log($"{gameObject.name} | Are {_currentNode.mapPosition} and {targetNode.mapPosition} equal? {_currentNode.Equals(targetNode)}");
            if (currentNode.Equals(targetNode))
            {
                pathSuccess = true;
                break;
            }

            // Search for the next best neighbor
            //Debug.Log($"{gameObject.name} | Finding neighbors...");
            foreach (MapNode neighbor in MapManager.Instance.GetNeighbors(currentNode))
            {
                if (!neighbor.tileData.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int _neighborCost = currentNode.gCost + NodeDistanceCost(currentNode, neighbor);
                if (_neighborCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = _neighborCost;
                    neighbor.hCost = NodeDistanceCost(neighbor, targetNode); // Maybe change to use Vector3Int.Distance
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Push(neighbor);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbor);
                    }
                }
            }

        }

        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        PathRequestManager.Instance.FinishedProcessingPath(waypoints, pathSuccess);
    }

    /// <summary>
    /// Retraces the path from the start node to the end node by following the parents from the end node.
    /// </summary>
    /// <param name="startNode">The starting node</param>
    /// <param name="endNode">The ending node</param>
    /// <returns>A list of Vector3s from start to end.</returns>
    private Vector3[] RetracePath(MapNode startNode, MapNode endNode)
    {
        List<MapNode> path = new List<MapNode>();
        MapNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);

        if (debug)
        {
            regPath = GeneratePath(path);

            Array.Reverse(regPath);

            simPath = SimplifyPath(path);

            Array.Reverse(simPath);

            return simPath;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    private Vector3[] GeneratePath(List<MapNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();

        for (int i = 0; i < path.Count; i++)
        {
            waypoints.Add(path[i].position + Vector3.one * 0.5f);
        }

        return waypoints.ToArray();
    }

    /// <summary>
    /// Simplifies a path of nodes by removing nodes which do not change direction.
    /// </summary>
    /// <param name="path">The path to simplify</param>
    /// <returns>A simplified path.</returns>
    private Vector3[] SimplifyPath(List<MapNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 directionNew = new Vector2(path[i].position.x - path[i + 1].position.x,
                                               path[i].position.y - path[i + 1].position.y);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].position + Vector3.one * 0.5f);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    /// <summary>
    /// Get euclidean distance cost between two nodes.
    /// </summary>
    /// <param name="nodeA">The starting node</param>
    /// <param name="nodeB">The ending node</param>
    /// <returns>The distance cost for traveling from one node to the next.</returns>
    private int NodeDistanceCost(MapNode nodeA, MapNode nodeB)
    {
        int _distX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int _distY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        if (_distX > _distY)
        {
            return diagonalMoveCost * _distY + adjacentMoveCost * (_distX - _distY);
        }

        return diagonalMoveCost * _distX + adjacentMoveCost * (_distY - _distX);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug && Application.isEditor)
        {
            return;
        }

        for (int i = 1; i < regPath.Length; i++)
        {
            Gizmos.color = regPathColor;
            Gizmos.DrawSphere(regPath[i], 0.05f);

            Gizmos.DrawLine(regPath[i - 1], regPath[i]);
        }

        for (int i = 1; i < simPath.Length; i++)
        {
            Gizmos.color = simPathColor;
            Gizmos.DrawSphere(simPath[i], 0.05f);

            Gizmos.DrawLine(simPath[i - 1], simPath[i]);
        }
    }
}
