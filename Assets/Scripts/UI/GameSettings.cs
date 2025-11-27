using System.Collections.Generic;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.UI;

namespace TTT.UI
{
    public class GameSettings : MonoBehaviour
    {
        public List<SettingsTabButton> settingButtons;
        public List<GameObject> tabPages;

        [SerializeField]
        public SettingsTabButton selectedTab;
        public Color tabIdleColor;
        public Color tabHoverColor;
        public Color tabActiveColor;

        // public SettingsPage settingsPage;

        public void Start()
        {
            // Select first tab
            foreach (SettingsTabButton SettingsTabButton in settingButtons)
            {
                if (SettingsTabButton.transform.GetSiblingIndex() == 0)
                    OnTabSelected(SettingsTabButton);
            }
        }

        public void Subscribe(SettingsTabButton button)
        {
            if (settingButtons == null)
            {
                settingButtons = new List<SettingsTabButton>();
            }

            settingButtons.Add(button);
        }

        public void OnTabEnter(SettingsTabButton SettingsTabButton)
        {
            ResetTabs();
            if ((selectedTab == null) || (SettingsTabButton != selectedTab))
                SettingsTabButton.background.color = tabHoverColor;
        }

        public void OnTabExit(SettingsTabButton SettingsTabButton)
        {
            ResetTabs();
        }

        public void OnTabSelected(SettingsTabButton SettingsTabButton)
        {
            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            Debug.Log("Selected Tab: " + SettingsTabButton.name);

            selectedTab = SettingsTabButton;

            selectedTab.Select();

            ResetTabs();

            Debug.Log("BG Color: " + SettingsTabButton.background.color);
            Debug.Log("Active Color: " + tabActiveColor);
            SettingsTabButton.background.color = tabActiveColor;
            Debug.Log("BG Color: " + SettingsTabButton.background.color);

            int index = SettingsTabButton.transform.GetSiblingIndex();
            for (int i = 0; i < tabPages.Count; i++)
            {
                if (i == index)
                {
                    tabPages[i].SetActive(true);
                }
                else
                {
                    tabPages[i].SetActive(false);
                }
            }
        }

        public void ResetTabs()
        {
            foreach (SettingsTabButton SettingsTabButton in settingButtons)
            {
                if ((selectedTab != null) && (SettingsTabButton == selectedTab))
                    continue;
                SettingsTabButton.background.color = tabIdleColor;
            }
        }

        public void NextTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int nextIndex =
                currentIndex < settingButtons.Count - 1
                    ? currentIndex + 1
                    : settingButtons.Count - 1;
            OnTabSelected(settingButtons[nextIndex]);
        }

        public void PreviousTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int previousIndex = currentIndex > 0 ? currentIndex - 1 : 0;
            OnTabSelected(settingButtons[previousIndex]);
        }
    }
}
