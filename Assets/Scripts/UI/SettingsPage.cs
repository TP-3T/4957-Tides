using TTT.UI;
using UnityEngine;

public class SettingsPage : MonoBehaviour
{
    public GameObject[] panels;

    public GameSettings settingsGroup;
    public int panelIndex;

    private void Start() { }

    // Update is called once per frame
    void Awake()
    {
        ShowCurrentPanel();
    }

    public void ShowCurrentPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].gameObject.SetActive(true);
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetPageIndex(int index)
    {
        panelIndex = index;
        ShowCurrentPanel();
    }
}
