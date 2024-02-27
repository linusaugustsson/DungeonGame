using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Generates a map with the cell prefab from a MapObject(SO), the mapObject is created from the MapEditor.
/// </summary>
public class MapLoader : MonoBehaviourRefSetup
{
    public MapData mapToGenerate;
    public Grid gridComponent;
    public SimplePlayerController playerPrefab;
    public Level currentLevel;

    static bool generatedStaticData;
    static public List<(string name, IResourceLocation location)> avalibleMaps = new();
    static public Cell cellPrefab;
    static public Action OnMapLoaderReady;
    static public bool MapLoaderReady = false;

    void Awake()
    {
        if(!generatedStaticData)
        {
            generatedStaticData = true;
            Addressables.InitializeAsync().Completed += OnAddressablesInitialized;
        }
    }

    async static private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> obj)
    {
        var mapResourceOP = Addressables.LoadResourceLocationsAsync(new List<string>(){ "Map", "Map-Generate" }, Addressables.MergeMode.Union, typeof(object));
        mapResourceOP.Completed += AvalibleMapsFetched;
        var cellPrefabOP = Addressables.LoadAssetAsync<GameObject>("CellPrefab");
        cellPrefabOP.Completed += CellPrefabLoaded;

        await Task.WhenAll(mapResourceOP.Task, cellPrefabOP.Task);

        MapLoaderReady = true;
        OnMapLoaderReady?.Invoke();

    }

    private static void CellPrefabLoaded(AsyncOperationHandle<GameObject> obj)
    {
        if(obj.Status == AsyncOperationStatus.Succeeded)
        {
            cellPrefab = obj.Result.GetComponent<Cell>();
        }
        else
        {
            throw new Exception("Could not fetch cell prefab from addressables.");
        }
    }

    static private void AvalibleMapsFetched(AsyncOperationHandle<IList<IResourceLocation>> obj)
    {
        if(obj.Status == AsyncOperationStatus.Succeeded)
        {
            var mapLocations = obj.Result;
            foreach (var mapLocation in mapLocations)
            {
                avalibleMaps.Add((mapLocation.PrimaryKey, mapLocation));
                avalibleMaps.Sort();
            }
        }
        else
        {
            throw new Exception("Could not load addressable maps");
        }
    }

    async static public Task<(GameObject, MapData)> LoadMapAsync(int index)
    {
        var tcs = new TaskCompletionSource<(GameObject, MapData)>();
        var mapToLoad = avalibleMaps[index].location;
        Addressables.LoadAssetAsync<object>(avalibleMaps[index].location).Completed += (obj) =>
        {
            (GameObject, MapData) result = (null, null);
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                if (obj.Result is MapData)
                {
                    result.Item2 = obj.Result as MapData;
                }
                else
                {
                    result.Item1 = obj.Result as GameObject;
                }
            }
            tcs.SetResult(result);
        };
        
        await tcs.Task;

        return tcs.Task.Result;
    }

    static Cell CreateCellObject(Vector3 worldPos, Grid gridComponent)
    {
        var cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, gridComponent.transform);
        return cell;
    }

    static private void SpawnCellObjects(string[] spawnCollection, Action<int, MapObject, GameObject> onAssetLoaded)
    {
        int nrOfCellPositions = spawnCollection.Length;
        for (int i = 0; i < nrOfCellPositions; i++)
        {
            string spawnStr = spawnCollection[i];
            if (spawnStr != "")
            {
                int index = i;
                AssetReference assetRef = new AssetReference(spawnStr);
                Addressables.LoadAssetAsync<MapObject>(assetRef).Completed += (op) =>
                {
                    var mapObj = op.Result;
                    var asset = mapObj.asset;

                    onAssetLoaded(index, mapObj, asset);
                };
            }
        }
    }

    public static void GenerateMap(MapData mapToGenerate, Level levelRootObject)
    {
        if (mapToGenerate == null) return;

        var mapParent = levelRootObject;
        mapParent.name = mapToGenerate.mapName;
        var gridComponent = mapParent.GetComponent<Grid>();
        gridComponent.cellSize = mapToGenerate.gridCellSize;

        foreach (var mapTile in mapToGenerate.mapSetup)
        {
            var tileData = mapTile.Value.tileData;

            var coord = gridComponent.GetCellCenterWorld(mapTile.Key);
            var tile = CreateCellObject(coord, gridComponent);
            tile.SetupCell(mapToGenerate.GetNeighborsDirectionFlag(mapTile.Key));

            //Spawn insideCell object ie spawn a object to a 3x3 grid inside the cell
            SpawnCellObjects(tileData.cellObjects, (i, mapObj, asset) =>
            {
                if (mapObj.attatchType == MapObject.GridAttachmentType.InsideCell)
                {
                    int x = i % 3;
                    int y = 2 - i / 3;
                    Vector3 spawnPos = tile.GetCellPlacement(x, y);
                    var assetObj = Instantiate(asset, spawnPos, asset.transform.rotation);
                }
            });

            //Spawn an object on an edge/wall.
            SpawnCellObjects(tileData.wallObjects, (i, mapObj, asset) =>
            {
                if (mapObj.attatchType != MapObject.GridAttachmentType.InsideCell)
                {
                    EditorCell.WallPosition wallPos = (EditorCell.WallPosition)i;
                    Quaternion rotation = Quaternion.Euler(0f, ((i + 2) % 4) * 90f, 0f); //Ex backWall should rotate in forward direction.
                    var assetObj = Instantiate(asset, tile.GetWallPosition(wallPos, out var wallTransform), asset.transform.rotation * rotation);

                    if (mapObj.attatchType == MapObject.GridAttachmentType.OnlyWallEdge)
                    {
                        assetObj.transform.SetParent(wallTransform, true);
                    }
                }
            });

        }

        if(levelRootObject)
        {
            levelRootObject.spawnPoints = mapToGenerate.spawnPoints;
            levelRootObject.levelName = mapToGenerate.mapName;

            if (FishNet.InstanceFinder.IsServer)
            {
                foreach (var key in mapToGenerate.mapSetup.Keys)
                {
                    levelRootObject.tileOccupancy.Add(key, false);
                }
            }
        }
        levelRootObject.mapData = mapToGenerate;
        levelRootObject.loadedMapData = true;
        SteamLobby.instance.sessionManager.spawnedLevel = levelRootObject;
        SteamLobby.instance.sessionManager.GeneratedMapCompleted();

    }

    override protected void SetupReferences()
    {
        gridComponent = GetComponent<Grid>();
    }

    [ContextMenu("Generate Map")]
    public void GenerateMapContextFunction()
    {
        GenerateMap(mapToGenerate, null);
    }
}
