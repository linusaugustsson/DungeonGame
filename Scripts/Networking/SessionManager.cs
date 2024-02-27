using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using Steamworks;
using FishNet.Connection;

public class SessionManager : NetworkBehaviour
{    public Level spawnedLevel;

    public enum SessionState
    {
        Lobby,
        InGame,
    }

    public int targetWins;
    int charactersDead = 0;


    public float winTime = 5.0f;


    public bool IsRoundOver()
    {
        return charactersDead >= allPlayers.Count - 1;
    }

    private void Awake()
    {
        CollectPlayers();
    }
    private void OnAnyDeath(Health healthComponent)
    {
        charactersDead++;
        if (IsRoundOver())
        {
            NetworkPlayerData roundWinner = GetWinner();

            if(roundWinner == null)
            {
                Debug.Log("No winner found, draw?");
                StartCoroutine(NextRoundAfterTime(winTime));
                return;
            }

            roundWinner.wins++;

            StartCoroutine(NextRoundAfterTime(winTime));
        }
    }

    private void OnDisable()
    {
        if (this.IsServer)
        {
            Health.OnAnyDeath -= OnAnyDeath;
        }
    }

    public GameObject myCharacter;

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (this.IsServer)
        {
            Health.OnAnyDeath += OnAnyDeath;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }


    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        SteamLobby.instance.sessionManager = this;
    }


    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if(IsHost)
        {
            ConnectionInfo.instance.ShowConnectionMessage("Closed session.", 3.0f);
        } else
        {
            ConnectionInfo.instance.ShowConnectionMessage("Connection lost.", 3.0f);
        }
        
        SteamLobby.instance.LeaveSteamLobby();
        MenuManager.instance.MainMenu();
    }
    /*
    public override void OnStopClient()
    {
        base.OnStopClient();
        ConnectionInfo.instance.ShowConnectionMessage("Connection lost.", 3.0f);
        SteamLobby.instance.LeaveSteamLobby();
        MenuManager.instance.MainMenu();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        ConnectionInfo.instance.ShowConnectionMessage("Connection lost.", 3.0f);
        SteamLobby.instance.LeaveSteamLobby();
        MenuManager.instance.MainMenu();
    }
    */

    private void Update()
    {
        if(levelTouple.HasValue)
        {
            var levelData = levelTouple.Value;
            if (IsOwner)
            {
                levelTouple = null;
                MenuManager.instance.CloseAllMenus();
                if (levelData.levelPrefab != null)
                {
                    spawnedLevel = Instantiate(levelData.levelPrefab).GetComponent<Level>();
                    SteamLobby.instance.networkManager.ServerManager.Spawn(spawnedLevel.gameObject, GetComponent<NetworkObject>().Owner);
                    InitalSpawnPlayers(spawnedLevel);
                }
                else if (levelData.mapData != null)
                {
                    spawnedLevel = Instantiate(levelPrefab);
                    spawnedLevel.mapIndex = levelData.mapIndex;
                    InstanceFinder.ServerManager.Spawn(spawnedLevel.gameObject); 
                    MapLoader.GenerateMap(levelData.mapData, spawnedLevel);
                    //ClientSpawnGeneratedMap(levelData.mapIndex);

                    //GeneratedMapCompleted();
                }
                else
                {
                    throw new System.Exception("LoadMapAsync returned no valid data.");
                }

            }
            else
            {
                //spawnedLevel = MapLoader.GenerateMap(levelData.mapData);
            }

        }

    }
    bool started = false;
    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true, RunLocally = false)]
    public void ClientSpawnGeneratedMap(int mapIndex)
    {
        FetchAndLoadMap(mapIndex);
    }
    int completedGenerations;
    [ServerRpc(RequireOwnership = false)]
    public void GeneratedMapCompleted()
    {
        completedGenerations++;
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        if (!started && completedGenerations >= networkPlayerDatas.Length)
        {
            started = true;
            InitalSpawnPlayers(spawnedLevel);
        }
    }

    public Level levelPrefab;
    (GameObject levelPrefab, MapData mapData, int mapIndex)? levelTouple;
    public void StartGame()
    {
        if (GetComponent<NetworkObject>().IsOwner == false)
        {
            return;
        }

        SteamMatchmaking.SetLobbyJoinable(new CSteamID(SteamLobby.instance.currentLobbyID), false);


        FindObjectOfType<SceneLoading>().LoadMap();

        //FetchAndLoadMap(MenuManager.instance.levelSelectDropdown.value);
    }

    private void FetchAndLoadMap(int mapIndex)
    {
        var loadMapTask = MapLoader.LoadMapAsync(mapIndex);
        loadMapTask.ContinueWith((task) =>
        {
            if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                var mapData = task.Result;
                levelTouple = (mapData.Item1, mapData.Item2, mapIndex);
            }

        });
    }

    public void InitalSpawnPlayers(Level map)
    {
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        
        for (int i = 0; i < networkPlayerDatas.Length; i++)
        {
            GameObject go = Instantiate(SteamLobby.instance.playerCharacter, spawnedLevel.GetSpawnPoint(i), Quaternion.identity);
            networkPlayerDatas[i].myPlayerController = go.GetComponent<SimplePlayerController>();
            networkPlayerDatas[i].spawnedCharacter = go.GetComponent<NetworkObject>();
            SteamLobby.instance.networkManager.ServerManager.Spawn(go, networkPlayerDatas[i].networkConnection);
        }

        MenuManager.instance.menuCamera.SetActive(false);
    }

    public List<NetworkPlayerData> allPlayers = new List<NetworkPlayerData>();
    public void CollectPlayers()
    {
        allPlayers.Clear();
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        for (int i = 0; i < networkPlayerDatas.Length; i++)
        {
            allPlayers.Add(networkPlayerDatas[i]);
        }
    }


    public void DestroyCorpses()
    {
        CollectPlayers();

        for (int i = 0; i < allPlayers.Count; i++)
        {
            allPlayers[i].spawnedCharacter.GetComponent<Health>().ClearCorpse();
        }
    }

    [ObserversRpc]
    public void NextRound()
    {
        charactersDead = 0;
        DestroyCorpses();
        ResetPlayers();
    }


    public void ResetPlayers()
    {
        if(IsOwner)
        {
            for(int i = 0; i < allPlayers.Count; i++)
            {
                allPlayers[i].spawnedCharacter.GetComponent<Health>().InitialState();
                allPlayers[i].spawnedCharacter.GetComponent<SimplePlayerController>().SetClientPosition(spawnedLevel.GetRandomSpawnPoint());
            }
        }
    }

    private NetworkPlayerData GetWinner()
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i] == allPlayers[i].spawnedCharacter.GetComponent<Health>().isAlive)
            {
                return allPlayers[i];
            }

        }
        return null;
    }


    IEnumerator NextRoundAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        NextRound();
    }

}
