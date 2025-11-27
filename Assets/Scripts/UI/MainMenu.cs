using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject Mainmenu;

    [SerializeField]
    private GameObject MultiplayerMenu;

    [SerializeField]
    private GameObject SettingsMenu;

    [SerializeField]
    private GameObject SinglePlayerMenu;

    [SerializeField]
    private GameObject MapBrowserMenu;

    [SerializeField]
    private GameObject JoinMenu;

    [SerializeField]
    private GameObject CreateMenu;

    [SerializeField]
    private GameObject GameUI;

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
    public void OpenSinglePlayerMenu()
    {
        Debug.Log("Clicked single player!");
        ChangeActiveMenu(SinglePlayerMenu);
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

    void Awake()
    {
        Debug.Log("Main Menu script is awake.");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentMenu = Mainmenu;
        SettingsMenu.SetActive(false);
        MultiplayerMenu.SetActive(false);
        SinglePlayerMenu.SetActive(false);
        MapBrowserMenu.SetActive(false);
        GameUI.SetActive(false);

        CurrentMenu.SetActive(true);
    }

    public void OpenMainMenu()
    {
        Debug.Log("Clicked Start Game!");
        ChangeActiveMenu(Mainmenu);
    }

    public void SillyFunction(string msg)
    {
        Debug.LogWarning(msg);
    }

    // Update is called once per frame
    void Update() { }
}
