using System;
using UnityEngine;
/// <summary>
/// Contains functionality for the EditorCells, which is attatched to every "tile" in the MapEditor.
/// Used for showing information about the cell to the developer.
/// </summary>




public class EditorCell : MonoBehaviourRefSetup
{
    public enum WallPosition
    {
        Forward,
        Right,
        Back,
        Left,
    }
    const int nrOfWalls = 4;
    const int nrOfFloorObjectPlacements = 9;
    [SerializeField, HideInInspector] TextMesh spawnText;
    [SerializeField, HideInInspector] GameObject[] floorCellObjects = new GameObject[nrOfFloorObjectPlacements];
    [SerializeField, HideInInspector] GameObject[] wallCellObjects = new GameObject[nrOfWalls];
    
    public void SetSpawnPoint(bool hasSpawnpoint)
    {
        spawnText.gameObject.SetActive(hasSpawnpoint);
    }


    public void SetFloorCellObjects(int index, bool status)
    {
        floorCellObjects[index].SetActive(status);
    }

    public WallPosition SetWallObject(WallPosition wallPos, bool status)
    {
        wallCellObjects[(int)wallPos].SetActive(status);
        return wallPos;
    }
    public WallPosition SetWallObject(DirectionFlag directionFlag, bool status)
    {
        WallPosition wallPos = DirectionToWallPosition(directionFlag);
        return SetWallObject(wallPos, status);
    }

    override protected void SetupReferences()
    {
        spawnText = transform.Find("Pivot/SpawnText").GetComponent<TextMesh>();
        if(floorCellObjects.Length < nrOfFloorObjectPlacements)
        {
            floorCellObjects = new GameObject[nrOfFloorObjectPlacements];
        }
        if(wallCellObjects.Length < nrOfWalls)
        {
            wallCellObjects = new GameObject[nrOfWalls];
        }
        var floorObjectsPivot = transform.Find("Pivot/FloorObjects");
        for (int i = 0; i < nrOfFloorObjectPlacements; i++)
        {
            floorCellObjects[i] = floorObjectsPivot.GetChild(i).gameObject;
        }
        var wallObjectParent = transform.Find("Pivot/WallObjects");

        for (int i = 0; i < nrOfWalls; i++)
        {
            wallCellObjects[i] = wallObjectParent.Find(((WallPosition)i).ToString() + "WallObj").gameObject;
        }
    }

    private static WallPosition DirectionToWallPosition(DirectionFlag directionFlag)
    {
        WallPosition wallPos = WallPosition.Forward;
        switch (directionFlag)
        {
            case DirectionFlag.Forward:
                wallPos = WallPosition.Forward;
                break;
            case DirectionFlag.Back:
                wallPos = WallPosition.Back;
                break;
            case DirectionFlag.Right:
                wallPos = WallPosition.Right;
                break;
            case DirectionFlag.Left:
                wallPos = WallPosition.Left;
                break;
            default:
                break;
        }

        return wallPos;
    }

}
