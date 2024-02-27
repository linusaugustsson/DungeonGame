using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Map/Create Map", order = 1)]
public class MapData : ScriptableObject
{
    [Serializable]
    public class TileData
    {
        public bool spawnPoint;
        public string[] wallObjects = new string[4];
        public string[] cellObjects = new string[9];
    }

    public static readonly (DirectionFlag directionEnum, Vector3Int directionVector)[] neigborDirectionsSet = 
    {
        (DirectionFlag.Forward, Vector3Int.forward),
        (DirectionFlag.Right, Vector3Int.right),
        (DirectionFlag.Back, Vector3Int.back),
        (DirectionFlag.Left, Vector3Int.left),
    };

    public string mapName;
    public int mapWidth;
    public int mapHeight;
    public Vector3Int gridCellSize = Vector3Int.one;
    public GridDataDictionary mapSetup = new GridDataDictionary();
    public List<Vector3Int> spawnPoints = new();

    public DirectionFlag GetNeighborsDirectionFlag(Vector3Int key)
    {
        DirectionFlag state = DirectionFlag.None;
        foreach (var set in neigborDirectionsSet)
        {
            if (mapSetup.TryGetValue(key + set.directionVector, out _))
            {
                state |= set.directionEnum;
            }
        }
        return state;
    }

    public List<Vector3Int> GetNeighbors(Vector3Int key)
    {
        List<Vector3Int> neighbours = new();
        foreach (var set in neigborDirectionsSet)
        {
            var coord = key + set.directionVector;
            if (mapSetup.TryGetValue(coord, out _))
            {
                neighbours.Add(coord);
            }
        }
        return neighbours;
    }

    static public Vector3Int GetDirectionVector(Vector3Int key, DirectionFlag direction)
    {
        Vector3Int directionVector = Array.Find(neigborDirectionsSet, (dirTouple) => dirTouple.directionEnum == direction).directionVector;
        return key + directionVector;
    }
}
