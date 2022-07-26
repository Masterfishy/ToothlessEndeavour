using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Default,
    Ground,
    Wall
}

public class MapManager : Singleton<MapManager>
{
    [Header("Tile Maps")]
    public Tilemap tileMap;
    public Tilemap walkableTileMap;
    public Tilemap unwalkableTileMap;

    [Header("Map Tiles")]
    public MapTile groundTile;
    public MapTile wallTile;

    private Dictionary<Vector3Int, MapNode> map;
    private BoundsInt mapBounds;

    /// <summary>
    /// The size of the map.
    /// </summary>
    public int MapSize
    {
        get
        {
            return mapBounds.size.x * mapBounds.size.y;
        }
    }

    /// <summary>
    /// Get the MapNode at the given position.
    /// </summary>
    /// <param name="position">The position to search</param>
    /// <returns>
    /// Returns the MapNode object at the position; null if is not in the map.
    /// </returns>
    public MapNode GetMapNodeFromPosition(Vector3Int position)
    {
        map.TryGetValue(position, out MapNode node);

        return node;
    }

    /// <summary>
    /// Gets adjacent and diagonal MapNodes to the given MapNode.
    /// </summary>
    /// <param name="node">The node of interest</param>
    /// <returns>A list of MapNodes diagonal and adjacent to the given node.</returns>
    public List<MapNode> GetNeighbors(MapNode node)
    {
        List<MapNode> neighbors = new List<MapNode>();

        Vector3Int locus = node.position;
        Vector3Int readPosition = Vector3Int.zero;

        for (int x = locus.x - 1; x <= locus.x + 1; x++)
        {
            for (int y = locus.y - 1; y <= locus.y + 1; y++)
            {
                if (x == locus.x && y == locus.y)
                {
                    continue;
                }

                readPosition.x = x;
                readPosition.y = y;

                map.TryGetValue(readPosition, out MapNode neighbor);

                if (neighbor != null && neighbor.tileData.walkable)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Converts a world position to a map position.
    /// </summary>
    /// <param name="position">The world position</param>
    /// <returns>The corresponding map position to the given world position</returns>
    public Vector3Int WorldPositionToMapPosition(Vector3 position)
    {
        return tileMap.WorldToCell(position);
    }

    private void Awake()
    {
        map = new Dictionary<Vector3Int, MapNode>();
        mapBounds = tileMap.cellBounds;

        Vector3Int readPosition = Vector3Int.zero;
        for (int x = mapBounds.xMin; x < mapBounds.xMax; x++)
        {
            for (int y = mapBounds.yMin; y < mapBounds.yMax; y++)
            {
                readPosition.x = x;
                readPosition.y = y;

                TileBase tileBase = tileMap.GetTile(readPosition);
                MapTile mapTile = TileBaseToMapTile(tileBase);

                AddTile(readPosition, mapTile);
            }
        }

        tileMap.ClearAllTiles();
    }

    /// <summary>
    /// Adds a tile to the map at the given position.
    /// </summary>
    /// <param name="position">The position to add a tile</param>
    /// <param name="mapTile">The type of tile to add</param>
    private void AddTile(Vector3Int position, MapTile mapTile)
    {
        if (map.ContainsKey(position))
        {
            map.Remove(position);
        }

        if (mapTile.walkable)
        {
            walkableTileMap.SetTile(position, mapTile.tileBase);
        }
        else
        {
            unwalkableTileMap.SetTile(position, mapTile.tileBase);
        }

        MapNode node = new MapNode(position, mapTile);
        map.Add(position, node);
    }

    /// <summary>
    /// Converts a TileBase to a MapTile.
    /// </summary>
    /// <param name="tileBase">The TileBase</param>
    /// <returns>The MapTile with the given TileBase; null if the TileBase isnt a MapTile.</returns>
    private MapTile TileBaseToMapTile(TileBase tileBase)
    {
        if (tileBase == groundTile.tileBase)
        {
            return groundTile;
        }

        if (tileBase == wallTile.tileBase)
        {
            return wallTile;
        }

        return default;
    }
}
