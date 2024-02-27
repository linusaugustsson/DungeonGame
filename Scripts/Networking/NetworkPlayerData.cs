using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Observing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{

    [SyncVar(OnChange = nameof(OnNameChange))] public string playerName;

    private PlayerLobbyItem playerLobbyItem;
    private ScoreboardItem playerScoreboardItem;

    public NetworkConnection networkConnection;

    [SyncVar(OnChange = nameof(OnReadyChange))] public bool readyLobby = false;

    [SyncVar(OnChange = nameof(OnClassChange))] public int selectedClass = 0;
    [SyncVar] public List<int> selectedAbilityIndexes = new();


    [SyncVar(OnChange = nameof(OnWinsChange))] public int wins = 0;

    

    [SyncVar(OnChange = nameof(OnSpawnedCharacterChange))] public NetworkObject spawnedCharacter;
    
    public SimplePlayerController myPlayerController;
    
    void OnSpawnedCharacterChange(NetworkObject prev, NetworkObject next, bool asServer)
    {
        if (asServer || !IsOwner) return;
        GetComponent<InputController>().NewCharacter(next);
    }


    private void Awake()
    {
        playerLobbyItem = Instantiate(MenuManager.instance.playerLobbyItem, MenuManager.instance.lobbyPlayerList.transform);
        playerLobbyItem.playerName.text = playerName;

        playerScoreboardItem = Instantiate(Scoreboard.instance.scoreboardItemPrefab, Scoreboard.instance.scoreboardParent);
        playerScoreboardItem.playerName.text = playerName;
        playerScoreboardItem.playerClass.text = Class.classDatabase[selectedClass].className;

        if (SteamLobby.instance.sessionManager != null)
        {
            SteamLobby.instance.sessionManager.CollectPlayers();
        }
    }

    public void OnNameChange(string prev, string next, bool asServer)
    {
        playerLobbyItem.playerName.text = next;
        playerScoreboardItem.playerName.text = next;

    }

    public void OnReadyChange(bool prev, bool next, bool asServer)
    {
        if(next == true)
        {
            playerLobbyItem.itemBackground.color = playerLobbyItem.readyColor;
        } else
        {
            playerLobbyItem.itemBackground.color = Color.white;
        }
    }

    public void OnWinsChange(int prev, int next, bool asServer)
    {
        playerScoreboardItem.playerScore.text = "Score: " + next;
        Scoreboard.instance.ReorderScoreboardItems();

    }

    public void OnClassChange(int prev, int next, bool asServer)
    {
        playerLobbyItem.playerClass.text = Class.classDatabase[next].className;
        playerScoreboardItem.playerClass.text = Class.classDatabase[next].className;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("OnStartServer");

        if(SteamLobby.instance.isHosting == false)
        {
            return;
        }
        if(SteamLobby.instance.sessionManager != null)
        {
            return;
        }

        GameObject go = Instantiate(SteamLobby.instance.sessionManagerPrefab);
        SteamLobby.instance.networkManager.ServerManager.Spawn(go, GetComponent<NetworkObject>().Owner);

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");


        playerName = SteamLobby.instance.mySteamName;
    }


    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Debug.Log("OnStartNetwork");

        networkConnection = GetComponent<NetworkObject>().Owner;

        FindObjectOfType<ServerInfoDebug>().UpdateInfo();

        MenuManager.instance.Inlobby();
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);

        if (IsOwner == false)
        {
            
        }
        else
        {
            UpdateName(SteamLobby.instance.mySteamName);
        }
    }

    [ServerRpc]
    private void UpdateName(string myName)
    {
        playerName = myName;
    }

    [ServerRpc]
    public void UpdateReady(bool setReady)
    {
        readyLobby = setReady;
    }

    [ServerRpc]
    public void UpdateClass(int classIndex, List<int> selectedAbilities)
    {
        selectedClass = classIndex;
        this.selectedAbilityIndexes = selectedAbilities;
    }
}
