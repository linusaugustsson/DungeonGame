using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyDataEntry : MonoBehaviour
{

    public CSteamID lobbyID;
    public string lobbyName;
    public TextMeshProUGUI lobbyNameText;

    public string steamId64;

    public void SetLobbyData()
    {


        if (lobbyName == "")
        {
            lobbyNameText.text = "Empty Name";
        }
        else
        {
            lobbyNameText.text = lobbyName;
        }

    }



    public void JoinLobby()
    {

        SteamLobby.instance.JoinLobby(lobbyID, steamId64);
    }

}
