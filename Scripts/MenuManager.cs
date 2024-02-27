using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using static TMPro.TMP_Dropdown;

public class MenuManager : MonoBehaviour
{

    public static MenuManager instance;


    public GameObject mainMenu;
    public GameObject joinLobby;
    public GameObject inLobby;


    public GameObject lobbyPlayerList;
    public PlayerLobbyItem playerLobbyItem;

    public GameObject menuCamera;


    public TMP_Dropdown levelSelectDropdown;
    public TMP_Dropdown classSelectionDropdown;

    public GameObject startGameButton;


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
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if(MapLoader.MapLoaderReady)
        {
            SetupAndShowMenu();
        }
        else
        {
            MapLoader.OnMapLoaderReady += SetupAndShowMenu;
        }

    }

    private void OnEnable()
    {
        if(AddressableHandler.addressablesLoaded)
        {
            OnAddressablesLoaded();
        }
        else
        {
            AddressableHandler.OnAddressablesLoaded += OnAddressablesLoaded;
        }
    }

    private void OnDisable()
    {
        AddressableHandler.OnAddressablesLoaded -= OnAddressablesLoaded;
    }

    public CharacterSheetMenu characterSheet;
    public void ShowCharacterSheet()
    {
        inLobby.SetActive(false);
        characterSheet.gameObject.SetActive(true);
        characterSheet.SetCharacter(classSelectionDropdown.value);
    }

    private void OnAddressablesLoaded()
    {
        classSelectionDropdown.ClearOptions();
        foreach (var clasess in Class.classDatabase)
        {
            classSelectionDropdown.options.Add(new OptionData(clasess.className));
        }
    }

    private void SetupAndShowMenu()
    {
        levelSelectDropdown.AddOptions(MapLoader.avalibleMaps.ConvertAll(tuple => tuple.name));
        MainMenu();
    }



    public void CloseAllMenus()
    {
        mainMenu.SetActive(false);
        joinLobby.SetActive(false);
        inLobby.SetActive(false);

    }



    public void MainMenu()
    {
        CloseAllMenus();
        mainMenu.SetActive(true);
    }

    public void JoinLobbies()
    {
        CloseAllMenus();
        joinLobby.SetActive(true);
    }

    public void Inlobby()
    {
        CloseAllMenus();

        

        if (SteamLobby.instance.isHosting == false)
        {
            levelSelectDropdown.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);

        }

        inLobby.SetActive(true);
    }

}
