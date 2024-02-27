using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{

    public static LobbiesList instance;


    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;


    public List<GameObject> listOfLobbies = new List<GameObject>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }



    public void GetListOfLobbies()
    {
        Debug.Log("Get List Of Lobbies");
        SteamLobby.instance.GetLobbiesList();
    }



    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result)
    {
        if(SteamLobby.instance.networkManager.ServerManager.Started)
        {
            return;
        }
        MenuManager.instance.JoinLobbies();
        Debug.Log("Display lobbies");
        for (int i = 0; i < lobbyIDs.Count; i++)
        {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createdItem = Instantiate(lobbyDataItemPrefab);
                LobbyDataEntry lobbyDataEntry = createdItem.GetComponent<LobbyDataEntry>();
                lobbyDataEntry.steamId64 = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "connectadress");
                lobbyDataEntry.lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;
                lobbyDataEntry.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "name");
                lobbyDataEntry.SetLobbyData();

                createdItem.transform.SetParent(lobbyListContent.transform);
                createdItem.transform.localScale = Vector3.one;
                createdItem.transform.localPosition = new Vector3(createdItem.transform.localPosition.x, createdItem.transform.localPosition.y, 0.0f);

                listOfLobbies.Add(createdItem);
            }
        }
    }


    public void DestroyLobbies()
    {
        Debug.Log("Destroy lobbies");
        foreach (GameObject lobbyItem in listOfLobbies)
        {
            Destroy(lobbyItem);
        }
        listOfLobbies.Clear();
    }

}
