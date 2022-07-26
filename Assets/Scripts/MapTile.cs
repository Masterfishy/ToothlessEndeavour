using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class MapTile : ScriptableObject
{
    public TileBase tileBase;
    public TileType tileType;
    public bool walkable;
}
