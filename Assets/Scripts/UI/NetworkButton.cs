using TTT.GameEvents;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    [SerializeField]
    public GameEvent newMapEvent;

    [SerializeField]
    private MainMenu MainMenu;
    
    private string selectedMap;

    void Start()
    {
        selectedMap = MainMenu.selectedMap;
    }



    public void OnMapLoad()
    {
        gameObject.SetActive(false);
        
        // Get the selected map from MainMenu and raise the event with it
        if (MainMenu.selectedMap != null)
        {
            // newMapEvent.Raise(new NewMapEventArgs() { DataFile = MainMenu.selectedMap });
        }
        else
        {
            Debug.LogError("[NetworkButton] No map selected in MainMenu");
        }
    }
}
