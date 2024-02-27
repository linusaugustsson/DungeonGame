using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Steamworks;
using System.Runtime.CompilerServices;

public class ServerInfoDebug : MonoBehaviour
{

    public TextMeshProUGUI roomID;
    //public TextMeshProUGUI roomPublicPrivate;
    public TextMeshProUGUI roomPlayerName;
    public TextMeshProUGUI roomPlayersConnected;

    public TextMeshProUGUI player1Name;
    public TextMeshProUGUI player2Name;

    private float updateTimer = 0.0f;
    private float maxUpdateTime = 1.0f;

    private void Update()
    {
        
        updateTimer += Time.deltaTime;
        
        if(updateTimer >= maxUpdateTime)
        {
            updateTimer = 0.0f;
            UpdateNames();
        }
    }

    private void SpawnPlayerCharacter()
    {

    }

    public void UpdateInfo()
    {
        roomID.text = "Lobby ID: " + SteamLobby.instance.currentLobbyID.ToString();
        roomPlayerName.text = "Player Name: " + SteamLobby.instance.mySteamName;
        roomPlayersConnected.text = "Players Connected: " + SteamMatchmaking.GetNumLobbyMembers(new CSteamID(SteamLobby.instance.currentLobbyID)).ToString() + "/" + SteamMatchmaking.GetLobbyMemberLimit(new CSteamID(SteamLobby.instance.currentLobbyID)).ToString();

        //List<NetworkPlayerData> networkPlayerDatas = new List<NetworkPlayerData>();
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        for(int i = 0; i < networkPlayerDatas.Length; i++)
        {
            if(i == 0)
            {
                player1Name.text = "Player 1: " + networkPlayerDatas[i].playerName;
            } else
            {
                player2Name.text = "Player 2: " + networkPlayerDatas[i].playerName;
            }
        }
    }

    public void UpdateNames()
    {
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        for (int i = 0; i < networkPlayerDatas.Length; i++)
        {
            if (i == 0)
            {
                player1Name.text = "Player 1: " + networkPlayerDatas[i].playerName;
            }
            else
            {
                player2Name.text = "Player 2: " + networkPlayerDatas[i].playerName;
            }
        }
    }
}
