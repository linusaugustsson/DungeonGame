using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Server;
using System;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;

    //Callbacks
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    // Lobbies Callbacks
    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;

    public List<CSteamID> lobbyIDs = new List<CSteamID>();

    // Variables
    public ulong currentLobbyID;
    private const string hostAddressKey = "HostAddress";

    private const int MAXPLAYERS = 4;
    public bool hasRequest = false;

    public string mySteamName = "";

    public NetworkManager networkManager;

    public GameObject sessionManagerPrefab;

    [HideInInspector]public SessionManager sessionManager;

    public GameObject playerCharacter;
    public GameObject enemy;

    public string mySteamId64;

    public string steamIdToJoin;

    public bool isHosting = false;

    public enum HostType
    {
        JoinHost,
        Friend
    }

    public HostType hostType = HostType.JoinHost;


    private void Start()
    {
        if (SteamManager.Initialized == false)
        {
            return;
        }

        if (instance == null)
        {
            instance = this;
        }

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);


        mySteamName = SteamFriends.GetPersonaName().ToString();
        mySteamId64 = SteamUser.GetSteamID().ToString();

        //networkManager.ClientManager.StartConnection(mySteamName);

        DontDestroyOnLoad(this);
    }


    public void HostLobby()
    {
        if (SteamManager.Initialized == false)
        {
            Debug.Log("Can't host lobby, SteamManager not initialized");
            return;
        }

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MAXPLAYERS);
    }

    public void HostLobbyFriends()
    {
        if (SteamManager.Initialized == false)
        {
            Debug.Log("Can't host lobby, SteamManager not initialized");
            return;
        }

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MAXPLAYERS);
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Could not create steam lobby!");
            return;
        }

        if (hostType == HostType.JoinHost)
        {
            Debug.Log("Created steam lobby with session name: " + mySteamName);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", mySteamName);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "connectadress", mySteamId64);

        }
        else
        {

            //currentLobbyID = callback.m_ulSteamIDLobby;
            Debug.Log("Created steam lobby with session name: " + mySteamName);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", mySteamName);
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "connectadress", mySteamId64);
        }

        isHosting = true;
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection(mySteamId64);

        //networkManager.ServerManager.

        Debug.Log("Lobby created successfully");
    }


    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby " + callback.m_steamIDLobby);
        hasRequest = true;
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

        ConnectionInfo.instance.ShowConnectionMessage("Connecting...", 1.0f);
    }

    public bool isInLobby = false;
    public bool waitForLobby = false;
    private void Update()
    {

    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobbyID = callback.m_ulSteamIDLobby;

        Debug.Log("Joined OnLobbyEntered " + currentLobbyID.ToString());




        //if(GetComponent<ServerManager>().)

        if (GetComponent<ServerManager>().Started == true)
        {
            Debug.Log("Is server");
            return;
        }

        ConnectionInfo.instance.ShowConnectionMessage("Connected", 1.0f);
        Debug.Log("Is Client");

        if (hasRequest == true)
        {
            if(steamIdToJoin != null)
            {
                MenuManager.instance.Inlobby();
                steamIdToJoin = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "connectadress");
                networkManager.ClientManager.StartConnection(steamIdToJoin);
            }
            
            return;
        }


        networkManager.ClientManager.StartConnection(steamIdToJoin);
        return;

        if (hasRequest == false)
        {
            return;
        }

        //string str = currentLobbyID.Substring(0, 5);

        Debug.Log("Has join request");

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            // Only do this if already in lobby
            // TODO ONLINE: Check if this is loaded too early or not. Should only try to connect when lobby has finished loading
            waitForLobby = true;
            Debug.Log("Not in lobby");
        }
        else
        {
            // Is already in the lobby
            Debug.Log("Already in lobby");
            Debug.Log("Connecting from invite with lobby ID: " + currentLobbyID.ToString());

            // SHOULD BE HOSTADRESS
            string roomID = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey);
            Debug.Log("RoomID " + roomID);
        }

    }




    public void ConnectFromInvite()
    {
        Debug.Log("Connecting from invite with lobby ID: " + currentLobbyID.ToString());

        string roomID = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), hostAddressKey);
        Debug.Log("RoomID " + roomID);

        ConnectionInfo.instance.ShowConnectionMessage("Connecting...", 1.0f);
    }


    static public string GetSteamName()
    {
        return SteamFriends.GetPersonaName().ToString();
    }

    public void LeaveSteamLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
    }

    public void GetLobbiesList()
    {

        Debug.Log("Get Lobbies List");
        if (lobbyIDs.Count > 0)
        {
            lobbyIDs.Clear();
        }

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();

    }

    public void JoinLobby(CSteamID lobbyID, string steamID)
    {
        steamIdToJoin = steamID;
        ConnectionInfo.instance.ShowConnectionMessage("Connecting...", 5.0f);
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    void OnGetLobbyList(LobbyMatchList_t result)
    {
        Debug.Log("On Get Lobby List");

        if (LobbiesList.instance.listOfLobbies.Count > 0)
        {
            LobbiesList.instance.DestroyLobbies();
        }

        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }

    }

    void OnGetLobbyData(LobbyDataUpdate_t result)
    {
        LobbiesList.instance.DisplayLobbies(lobbyIDs, result);
    }


    public void StartGame()
    {
        if(AllReady())
        {
            sessionManager.StartGame();
        }
        
    }
    public CharacterSheetMenu characterSheet;
    public void SetMyClass(int setClass)
    {
        sessionManager.CollectPlayers();

        for (int i = 0; i < sessionManager.allPlayers.Count; i++)
        {
            if (sessionManager.allPlayers[i].IsOwner)
            {
                Class classToSet = Class.classDatabase[setClass];
                int maxNrOfAbilities = classToSet.avalibleClassAbilities.Count;
                sessionManager.allPlayers[i].UpdateClass(setClass, classToSet.avalibleClassAbilities.ConvertAll<int>((ca) => Ability.abilityDatabase.FindIndex(a => a == ca)).GetRange(0, Math.Min(classToSet.nrOfAbilitySlots, maxNrOfAbilities)));
            }
        }
    }

    public void SetMyReady()
    {
        sessionManager.CollectPlayers();

        for (int i = 0; i < sessionManager.allPlayers.Count; i++)
        {
            if (sessionManager.allPlayers[i].IsOwner)
            {
                sessionManager.allPlayers[i].UpdateReady(!sessionManager.allPlayers[i].readyLobby);
            }
        }
    }


    public bool AllReady()
    {
        for(int i = 0; i < sessionManager.allPlayers.Count; i++)
        {
            if (sessionManager.allPlayers[i].readyLobby == false) {
                return false;
            }
        }

        return true;
    }

}
