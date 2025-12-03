using TTT.GameEvents;
using TTT.Helpers;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject Mainmenu;

    public string selectedMap;

    [SerializeField]
    private GameObject MultiplayerMenu;

    [SerializeField]
    private GameObject SettingsMenu;

    [SerializeField]
    private GameObject LobbyRoom;

    [SerializeField]
    private GameObject MapBrowserMenu;

    [SerializeField]
    private GameObject JoinMenu;

    [SerializeField]
    private GameObject CreateMenu;

    [SerializeField]
    private GameObject GameUI;

    [SerializeField]
    private GameObject LoadingScreen;

    [SerializeField]
    private int MapID;

    [SerializeField]
    private GameEvent startNetworkEvent;

    [SerializeField]
    private GameObject MenuBackground;

    private GameObject CurrentMenu;
    private GameObject previousMenu;

    /// <summary>
    /// On click to open the join menu
    /// </summary>
    public void OpenJoinMenu()
    {
        Debug.Log("Clicked join button!");
        ChangeActiveMenu(JoinMenu);
    }

    /// <summary>
    /// On click to open the join menu
    /// </summary>
    public void OpenCreateMenu()
    {
        Debug.Log("Clicked create button!");
        ChangeActiveMenu(CreateMenu);
    }

    /// <summary>
    /// On click to open single player menu.
    /// </summary>
    public void OpenLobbyRoomMenu()
    {
        Debug.Log("Clicked single player!");
        ChangeActiveMenu(LobbyRoom);
    }

    /// <summary>
    /// On click Open settings menu.
    /// </summary>
    public void OpenSettingsMenu()
    {
        Debug.Log("Clicked settings!");
        ChangeActiveMenu(SettingsMenu);
    }

    /// <summary>
    /// On click to open multiplayer menu.
    /// </summary>
    public void OpenMultiplayerMenu()
    {
        Debug.Log("Clicked multiplayer!");
        ChangeActiveMenu(MultiplayerMenu);
    }

    /// <summary>
    /// On click to go back to the previous menu.
    /// </summary>
    public void BackToPreviousMenu()
    {
        ChangeActiveMenu(previousMenu);
    }

    /// <summary>
    /// Opens map browser menu on click.
    /// </summary>
    public void OpenMapBrowserMenu()
    {
        Debug.Log("Clicked map browser!");
        ChangeActiveMenu(MapBrowserMenu);
    }

    /// <summary>
    /// Opens Game UI on start
    /// </summary>
    public void StartGameMenu()
    {
        Debug.Log("Clicked Start Game!");
        ChangeActiveMenu(LoadingScreen);
        MenuBackground.SetActive(true);
        // Defer map loading until after network starts (handled post-host/client start)
    }

    public void OpenGameUI()
    {
        ChangeActiveMenu(GameUI);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    /// <summary>
    /// Changes the active menu to the specified menu.
    /// </summary>
    /// <param name="menu"></param>
    public void ChangeActiveMenu(GameObject menu)
    {
        CurrentMenu.SetActive(false);
        menu.SetActive(true);
        previousMenu = CurrentMenu;
        CurrentMenu = menu;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentMenu = Mainmenu;
        SettingsMenu.SetActive(false);
        MultiplayerMenu.SetActive(false);
        LobbyRoom.SetActive(false);
        MapBrowserMenu.SetActive(false);
        GameUI.SetActive(false);
        LoadingScreen.SetActive(false);

        CurrentMenu.SetActive(true);
    }

    /// <summary>
    /// Opens the main menu
    /// </summary>
    public void OpenMainMenu()
    {
        Debug.Log("Clicked Start Game!");
        ChangeActiveMenu(Mainmenu);
    }

    public void SillyFunction(string msg)
    {
        Debug.LogWarning(msg);
    }


    /// <summary>
    /// Raises startNetworkEvent and starts as host with default IP and Port.
    /// Uses listen address "0.0.0.0" to listen on all network interfaces.
    /// </summary>
    public void StartHost()
    {
        if (startNetworkEvent != null)
        {
            var args = ScriptableObject.CreateInstance<StartNetworkEventArgs>();
            args.IsHost = true;
            args.Ip = "127.0.0.1";
            args.Port = 6767;
            startNetworkEvent.Raise(args);
        }
    }

    /// <summary>
    /// Raises startNetworkEvent and starts as client with specified IP and Port.
    /// Uses SetConnectionData to configure the UnityTransport component.
    /// </summary>
    public void StartClient(string ipAddress, ushort port)
    {
        if (startNetworkEvent != null)
        {
            var args = ScriptableObject.CreateInstance<StartNetworkEventArgs>();
            args.IsHost = false;
            args.Ip = ipAddress;
            args.Port = port;
            startNetworkEvent.Raise(args);
        }
    }

}
