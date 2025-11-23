using System.Collections.Generic;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Tooltip("The prefabs for menu button as Tabs")]
    public GameObject[] MenuButtonPrefab;

    

    private readonly Dictionary<string, GameObject> TabButtons = new()
    {
        { "General", new GameObject() },
        { "Controls", new GameObject() },
        { "Screen", new GameObject() },
        { "Audio", new GameObject() },
    };

    //In case I need to sort the lists by GetSiblingIndex
    //objListOrder.Sort((x, y) => x.OrderDate.CompareTo(y.OrderDate));

    public Color tabIdleColor;
    public Color tabHoverColor;
    public Color tabSelectedColor;
    private GameObject selectedTab;

    public void Start()
    {
        // ITERATE THROUGH
        // ASSIGN EACH BUTTON THE BUTTON NAME
        // on
    }

    // onClick() if the button name matches, open the tab?

    private void InitializeSettingsTabs() { }
}
