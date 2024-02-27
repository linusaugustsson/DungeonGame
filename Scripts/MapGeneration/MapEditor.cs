using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[Serializable]
public abstract class  MonoBehaviourRefSetup : MonoBehaviour
{
    [ContextMenu("Setup References")]
    public void Setup()
    {
        this.SetupReferences();
        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif

    }
    abstract protected void SetupReferences();
}




[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // Method called before serialization
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // Method called after deserialization
    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
        {
            throw new Exception($"The number of keys ({keys.Count}) does not match the number of values ({values.Count}).");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            Add(keys[i], values[i]);
        }
    }
}


[Serializable]
public class GridDataDictionary : SerializableDictionary<Vector3Int, GridData> { }

[Serializable]
public class GridData
{
    public EditorCell tileObject;
    public MapData.TileData tileData = new();
}

/// <summary>
/// Used to create tile based maps, creates MapObject scriptable objects for now, which can be used in the MapLoader component to generate the maps on the fly.
/// Support for pre-generation in future.
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
# endif

public class MapEditor : MonoBehaviourRefSetup
{
    override protected void SetupReferences()
    {
        gridComponent = GetComponent<Grid>();
        gridTransform = transform;
    }

    enum MapEditorMode
    {
        None,
        Room,
        SpawnPoint,
        MapObjects,
    }


    [SerializeField] EditorCell editorCellPrefab;
    [SerializeField]string mapName;

    [SerializeField, HideInInspector] Grid gridComponent;
    [SerializeField, HideInInspector] Transform gridTransform;
    [SerializeField] MapData currentMap;
    [SerializeField][HideInInspector] MapData lastMap;



    MapEditorMode currentMode;
    bool isSetupDone = false;
    BoundsInt mapBounds;


    private void OnDrawGizmos()
    {
        if (currentMap == null) return;
        Gizmos.color = Color.black;
        float cellSize = gridComponent.cellSize.x;
        // Calculate the size of the grid in world space
        float width = currentMap.mapWidth * cellSize;
        float height = currentMap.mapHeight * cellSize;

        // Calculate the starting position of the grid
        Vector3 gridOrigin = transform.position - new Vector3(width / 2f, 0f, height / 2f);

        for (int y = 0; y <= currentMap.mapHeight; y++)
        {
            Vector3 start = gridOrigin + new Vector3(0, 0, y * cellSize);
            Vector3 end = start + new Vector3(width, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= currentMap.mapWidth; x++)
        {
            Vector3 start = gridOrigin + new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = start + new Vector3(0f, 0f, height);
            Gizmos.DrawLine(start, end);
        }        
    }

    private void OnDisabled()
    {
        #if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= ClearAllSelections;
            EditorSceneManager.sceneOpened -= OnSceneChanged;
        #endif
    }
    private void ClearAllSelections()
    {
        if (this == null) return;
        #if UNITY_EDITOR
            Selection.activeGameObject = gameObject;
        #endif
    }

    static public List<MapObject> mapObjects = new List<MapObject>();
    bool hasFetchedMapObjects;
    MapEditor()
    {
#if UNITY_EDITOR

        if (!isSetupDone)
        {
            isSetupDone = true;
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += ClearAllSelections;
            EditorSceneManager.sceneOpened += OnSceneChanged;
        }
        #endif

    }

    private void MapObjectsLoaded(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<MapObject>> obj)
    {
        if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            mapObjects = new(obj.Result);
        }
    }

    [ContextMenu("Reimport Map Objects")]
    private void ReimportMapObjects()
    {
        hasFetchedMapObjects = false;
    }

#if UNITY_EDITOR
    private void OnSceneChanged(Scene scene, OpenSceneMode mode)
    {

        if(this == null)
        {
            OnDisabled();
        }
        else
        {
            RegenerateCurrentEditorMap();
        }

    }

    private void OnEnable()
    {
        if (Application.isPlaying) return;
        if (this == null)
        {
            OnDisabled();
        }
        else
        {
            RegenerateCurrentEditorMap();
        }
    }

    private void RegenerateCurrentEditorMap()
    {
        RemoveAllSceneTiles();
        CreateSceneTiles();
        EditorUtility.SetDirty(this);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (lastMap != currentMap)
            {
                lastMap = currentMap;
                EditorApplication.delayCall += () =>
                {
                    RegenerateCurrentEditorMap();
                };

            }
            if (currentMap == null) return;
            mapBounds = new BoundsInt(gridComponent.WorldToCell(transform.position) - new Vector3Int(currentMap.mapWidth / 2, 0, currentMap.mapHeight / 2), new Vector3Int(currentMap.mapWidth, 999, currentMap.mapHeight));
        }
    }

    private void CreateSceneTiles()
    {
        foreach (var key in currentMap.mapSetup.Keys)
        {
            var tileData = currentMap.mapSetup[key].tileData;
            var tileObject = currentMap.mapSetup[key].tileObject = CreateTileObject(gridComponent.GetCellCenterWorld(key));
            tileObject.SetSpawnPoint(tileData.spawnPoint);
            int nrOfwallObjects = tileData.wallObjects.Length;
            for (int i = 0; i < nrOfwallObjects; i++)
            {
                tileObject.SetWallObject((EditorCell.WallPosition)i, !string.IsNullOrEmpty(tileData.wallObjects[i]));
            }
            int nrOfCellObjects = tileData.cellObjects.Length;
            for (int i = 0; i < nrOfCellObjects; i++)
            {
                tileObject.SetFloorCellObjects(i, !string.IsNullOrEmpty(tileData.cellObjects[i]));
            }
        }
    }
#endif



    private void RemoveAllSceneTiles()
    {
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridTransform.GetChild(i).gameObject);
        }
    }


    private void SetEditorMode(MapEditorMode mode)
    {
        currentMode = mode;
    }
#if UNITY_EDITOR

    private void OnSceneGUI(SceneView obj)
    {
        if(this == null)
        {
            OnDisabled();
            return;
        }
        if(!hasFetchedMapObjects)
        {

            hasFetchedMapObjects = true;
            var op = Addressables.LoadAssetsAsync<MapObject>("MapObject", null);
            var compOP = op.WaitForCompletion();
            mapObjects = new List<MapObject>(compOP);
        }
        SetCorrectViewAndLockRotation();
        if (currentMap == null) return;

        DrawUI();

        HandleMapInput();
    }

    private void HandleMapInput()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown)
        {
            bool isLeft = currentEvent.button == 0;
            bool isRight = currentEvent.button == 1;
            Vector3 mousePosition = currentEvent.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            RaycastHit hit;

            if ((isLeft || isRight) && Physics.Raycast(ray, out hit))
            {
                var cellCoord = gridComponent.WorldToCell(hit.point.ZeroOutHeight());

                if (currentMode != MapEditorMode.None)
                {
                    bool changed = false;
                    switch (currentMode)
                    {
                        case MapEditorMode.Room:
                            if (isLeft)
                            {
                                changed = AddTile(cellCoord);
                            }
                            else if (isRight)
                            {
                                changed = RemoveTile(cellCoord);
                                currentMap.spawnPoints.Remove(cellCoord);
                            }
                            break;
                        case MapEditorMode.SpawnPoint:
                            if (currentMap.mapSetup.TryGetValue(cellCoord, out GridData editorCell))
                            {
                                editorCell.tileData.spawnPoint = isLeft;
                                editorCell.tileObject.SetSpawnPoint(isLeft);
                                if (isLeft)
                                {
                                    currentMap.spawnPoints.Add(cellCoord);
                                }
                                else
                                {
                                    currentMap.spawnPoints.Remove(cellCoord);
                                }
                                changed = true;
                            }
                            break;
                        case MapEditorMode.MapObjects:
                            if (currentMapObject != null && currentMap.mapSetup.TryGetValue(cellCoord, out GridData cell))
                            {
                                var cellCenterWorldPos = gridComponent.GetCellCenterWorld(cellCoord);
                                var cellCenterToRayHit = (hit.point - cellCenterWorldPos).ZeroOutHeight();

                                if(currentMapObject.attatchType == MapObject.GridAttachmentType.InsideCell)
                                {
                                    // Left = 0, Mid = 0.25, Right = 0.5 | *3 = 0, 1, 2 rounded to int.
                                    Vector3 range = (cellCenterToRayHit + Vector3.one * 0.25f) * 3;
                                    int x = Mathf.RoundToInt(range.x), y = 2 - Mathf.RoundToInt(range.z);
                                    int index = y * 3 + x;
                                    cell.tileObject.SetFloorCellObjects(index, isLeft);

                                    if (string.IsNullOrEmpty(cell.tileData.cellObjects[index]) && isLeft)
                                    {
                                        AssetReference assetRef = new AssetReference();
                                        assetRef.SetEditorAsset(currentMapObject);
                                        cell.tileData.cellObjects[index] = assetRef.AssetGUID;
                                        changed = true;
                                    }
                                    else if(!string.IsNullOrEmpty(cell.tileData.cellObjects[index]) && !isLeft)
                                    {
                                        cell.tileData.cellObjects[index] = "";
                                        changed = true;
                                    }
                                }
                                else
                                {
                                    changed = TryPlaceOnEdge(isLeft, cellCoord, changed, cell, cellCenterToRayHit);

                                }
                            }
                            break;
                    }
                    if (changed)
                    {
                        EditorUtility.SetDirty(this);
                        EditorUtility.SetDirty(currentMap);
                    }

                }
            }
        }
    }

    private bool TryPlaceOnEdge(bool isLeft, Vector3Int cellCoord, bool changed, GridData cell, Vector3 cellCenterToRayHit)
    {
        DirectionFlag state = DirectionFlag.None;

        bool right = (cellCenterToRayHit.x > 0.25f);
        bool left = cellCenterToRayHit.x < -0.25f;
        bool forward = cellCenterToRayHit.z > 0.25f;
        bool back = cellCenterToRayHit.z < -0.25f;
        if (right)
            state |= DirectionFlag.Right;
        if (left)
            state |= DirectionFlag.Left;
        if (forward)
            state |= DirectionFlag.Forward;
        if (back)
            state |= DirectionFlag.Back;

        if (IsAllowedToPlaceOnEdge(state, currentMap.GetNeighborsDirectionFlag(cellCoord), currentMapObject.attatchType, cellCoord))
        {
            var wallPos = cell.tileObject.SetWallObject(state, isLeft);
            if(string.IsNullOrEmpty(cell.tileData.wallObjects[(int)wallPos]) && isLeft)
            {
                AssetReference assetRef = new AssetReference();
                assetRef.SetEditorAsset(currentMapObject);
                string guid = assetRef.AssetGUID;
                cell.tileData.wallObjects[(int)wallPos] = guid;
                changed = true;

            }
            else if (!string.IsNullOrEmpty(cell.tileData.wallObjects[(int)wallPos]) && !isLeft)
            {
                cell.tileData.wallObjects[(int)wallPos] = "";
                changed = true;
            }

        }

        return changed;
    }

    const int directionMask = (1 << 4) - 1;
    bool IsAllowedToPlaceOnEdge(DirectionFlag placementDirectionInCell, DirectionFlag neigbors, MapObject.GridAttachmentType attachmentType, Vector3Int cellCoord)
    {
        bool baseCaseEdgeAttatchment = (placementDirectionInCell != 0) && ((placementDirectionInCell & (placementDirectionInCell - 1)) == 0); //If flag only contains one enum value, (Ie not corners or similar) set wallObject active depending on left/right click.
        if (attachmentType != MapObject.GridAttachmentType.InsideCell && !baseCaseEdgeAttatchment) return false;

        bool attatchmentRuleMet = false;


        switch (attachmentType)
        {
            case MapObject.GridAttachmentType.AnyCellEdge:
                attatchmentRuleMet = true;
                break;
            case MapObject.GridAttachmentType.OnlyWallEdge://Only allow to place in direction where no neigbors exists(Ie there is a wall in direction)
                attatchmentRuleMet = (neigbors & placementDirectionInCell) == 0; //If no bits are at same position, rule is met.
                break;
            case MapObject.GridAttachmentType.OnlyCellConnector: //Only allow to place in directions that have cells, ie there is no walls between the cells.
                attatchmentRuleMet = (neigbors & placementDirectionInCell) != 0; //if one bit are similar then rule is met.
                break;
            case MapObject.GridAttachmentType.DualCellConnector: //Only allow to place towards a cell, and must be walls in perpendicular direction.
                Vector3Int dirNeighborCoord = MapData.GetDirectionVector(cellCoord, placementDirectionInCell);
                var dirNeighborHood = currentMap.GetNeighborsDirectionFlag(dirNeighborCoord); 
                DirectionFlag LeftShiftCircular(DirectionFlag direction, int shiftAmount = 1)
                {
                    return (DirectionFlag)(((int)direction << shiftAmount) | ((int)direction >> (4 - shiftAmount)) & directionMask);
                }
                DirectionFlag RightShiftCircular(DirectionFlag direction, int shiftAmount = 1)
                {
                    int dir = (int)direction;
                    return (DirectionFlag)((dir >> shiftAmount) | (dir << (4 - shiftAmount)) & directionMask);
                }
                DirectionFlag perpendicularDirections = LeftShiftCircular(placementDirectionInCell) | RightShiftCircular(placementDirectionInCell);
                attatchmentRuleMet = (neigbors & placementDirectionInCell) != 0 && (neigbors & perpendicularDirections) == 0 ; //PlacementDirection needs to have a cell, perpendicular to placemnetDirection needs to have Walls/No cells.
                break;
            case MapObject.GridAttachmentType.InsideCell:
                attatchmentRuleMet = true;
                break;
            default:
                break;
        }

        return attatchmentRuleMet;
    }

    MapObject currentMapObject = null;

    private void DrawUI()
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 100, 500));

        //Draw editor buttons
        foreach (MapEditorMode mode in Enum.GetValues(typeof(MapEditorMode)))
        {
            if (GUILayout.Button(mode.ToString()))
            {
                SetEditorMode(mode);
            }
        }

        if(currentMode == MapEditorMode.MapObjects && mapObjects.Count > 0)
        {
            foreach (var mapObject in mapObjects)
            {
                if (GUILayout.Button(mapObject.name))
                {
                    currentMapObject = mapObject;
                }
            }

        }

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private static void SetCorrectViewAndLockRotation()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if(sceneView != null)
        {
            sceneView.isRotationLocked = false;
            sceneView.rotation = Quaternion.Euler(90, 0, 0);
            sceneView.orthographic = true;
            sceneView.isRotationLocked = true;
        }
    }

    private EditorCell CreateTileObject(Vector3 worldPos)
    {
        var tile = Instantiate(editorCellPrefab, worldPos, Quaternion.identity, gridComponent.transform);
        return tile;
    }

    public bool AddTile(Vector3Int cellPos)
    {
        bool added = false;
        if(mapBounds.Contains(cellPos) && !currentMap.mapSetup.TryGetValue(cellPos, out _))
        {
            added = true;
            var gridData = new GridData();
            print("added at: " + cellPos.ToString());
            var worldPos = gridComponent.GetCellCenterWorld(cellPos);
            gridData.tileObject = CreateTileObject(worldPos);
            currentMap.mapSetup.Add(cellPos, gridData);

            EditorUtility.SetDirty(this);
        }
        return added;
    }



    public bool RemoveTile(Vector3Int cellPos)
    {
        bool removed = false;
        if(currentMap.mapSetup.TryGetValue(cellPos, out GridData tileData))
        {
            print("Removed at: " + cellPos.ToString());
            if(tileData.tileObject)
            {
                DestroyImmediate(tileData.tileObject.gameObject);
            }
            removed = true;
            currentMap.mapSetup.Remove(cellPos);
        }
        return removed;
    }

    #region context functions
    [ContextMenu("Clean Grid")]
    private void CleanGrid()
    {
        if (currentMap == null) return;
        currentMap.mapSetup.Clear();
        RemoveAllSceneTiles();
    }



    [ContextMenu("Create new map")]
    private void CreateNewMap()
    {
        MapData newMap = ScriptableObject.CreateInstance<MapData>();
        newMap.mapHeight = 30;
        newMap.mapWidth = 30;
        newMap.mapName = mapName;
        newMap.mapSetup = new GridDataDictionary();

        string path = "Assets/Assets/Maps/";
        string filePath = path + mapName + ".asset";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        AssetDatabase.CreateAsset(newMap, filePath);
        AssetDatabase.SaveAssets();
        RemoveAllSceneTiles();
        currentMap = newMap;
    }
    #endregion
#endif


}

public static class Vector3Extension
{
    public static Vector3Int To3D(this Vector2Int vector, int y = 0)
    {
        return new Vector3Int(vector.x, y, vector.y);
    }

    public static Vector3 ZeroOutHeight(this Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }
}