using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : NetworkBehaviour
{
    public string levelName;
    public List<Vector3Int> spawnPoints = new();
    [SyncObject]
    public readonly SyncDictionary<Vector3Int, bool> tileOccupancy = new();
    [SyncVar]
    public int mapIndex;

    private Grid gridComponent;
    public MapData mapData;
    public bool loadedMapData;

    public Action<Vector3Int> OnRejectedTileMovement;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (FishNet.InstanceFinder.IsServer)
        {
            foreach (var key in mapData.mapSetup.Keys)
            {
                tileOccupancy.Add(key, false);
            }
        }

        if (base.IsServer) return; //Server map generation is already handled, therefor we skip it.
        var loadMapTask = MapLoader.LoadMapAsync(mapIndex);
        loadMapTask.ContinueWith((task) =>
        {
            if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                mapData = task.Result.Item2;
            }

        });
    }

    private void Update()
    {
        if (base.IsServer) return; //Server map generation is already handled, therefor we skip it.
        if (!loadedMapData && mapData != null)
        {
            loadedMapData = true;
            MapLoader.GenerateMap(mapData, this);
        }
    }

    private void Awake()
    {
        gridComponent = GetComponent<Grid>();
        FindObjectOfType<SessionManager>().spawnedLevel = this;
        FindObjectOfType<SessionManager>().InitalSpawnPlayers(this);
    }
    public Vector3 GetRandomSpawnPoint()
    {
        int index = UnityEngine.Random.Range(0, spawnPoints.Count);
        return GetSpawnPoint(index);
    }

    public Vector3 GetSpawnPoint(int spawnPointIndex)
    {
        if (spawnPoints.Count == 0) return Vector3.zero;
        return gridComponent.GetCellCenterWorld(spawnPoints[spawnPointIndex % spawnPoints.Count]);
    }

    public bool CanMoveToTile(Vector3Int to)
    {
        return tileOccupancy.TryGetValue(to, out bool occupied) && !occupied;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ModifyOccupancy(Vector3Int fromPos, Vector3Int toPos, bool noCheck = false, NetworkConnection caller = null)
    {

        if (!noCheck && tileOccupancy[toPos])
        {
            RejectTileMovement(caller, fromPos);
        }
        else
        {
            tileOccupancy[toPos] = true;
            tileOccupancy[fromPos] = false;
        }
    }

    [TargetRpc]
    private void RejectTileMovement(NetworkConnection plyConnection, Vector3Int fromPos)
    {
        OnRejectedTileMovement?.Invoke(fromPos);
    }

    internal Vector3Int PosToTile(Vector3 position, out Vector3 tileWorldPos)
    {
        Vector3Int tilePos = gridComponent.WorldToCell(position);
        tileWorldPos = gridComponent.GetCellCenterWorld(tilePos);
        return tilePos;
    }

    internal Vector3 TileToWorldPos(Vector3Int tileCoord)
    {
        return gridComponent.GetCellCenterWorld(tileCoord);
    }
}
