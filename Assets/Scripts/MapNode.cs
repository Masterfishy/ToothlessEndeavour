using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : IHeapItem<MapNode>
{
    public Vector3Int position;
    public MapTile tileData;

    public int gCost;
    public int hCost;

    public MapNode parent;

    private int heapIndex;

    public MapNode(Vector3Int _position, MapTile _tile)
    {
        position = _position;
        tileData = _tile;
    }

    public int Cost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex 
    { 
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    /// <summary>
    /// Compares two nodes based on their costs.
    /// </summary>
    /// <remarks>
    /// Uses hCost for tie breakers.
    /// </remarks>
    /// <param name="other">The other node to compare this node to</param>
    /// <returns>Return 1 if the current item has higher priority than the one we are comparing to.</returns>
    public int CompareTo(MapNode other)
    {
        int compare = Cost.CompareTo(other.Cost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}
