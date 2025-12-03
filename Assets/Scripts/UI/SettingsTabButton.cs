using TTT.GameEvents;
using UnityEngine;
using UnityEngine.UI;

namespace TTT.UI
{
    public class SettingsTabButton : MonoBehaviour
    {
        // TODO: HOOK UP MAIN MENU EVENT LISTENERS, CREATE EVENTS FOR MAIN MENU
        public GameSettings settingGroup;
        public GameEvent gameSettingsEvent;
        public GameEvent mainMenuEvent;

        [HideInInspector]
        public Image background;

        void Start()
        {
            background = GetComponent<Image>();
            if (settingGroup != null)
                settingGroup.Subscribe(this);
        }

        public void OnPointerClick(UnityEngine.Object eventArgs)
        {
            settingGroup.OnTabSelected(this);
        }

        public void OnPointerEnter(UnityEngine.Object eventArgs)
        {
            settingGroup.OnTabEnter(this);
        }

        public void OnPointerExit(UnityEngine.Object eventArgs)
        {
            settingGroup.OnTabExit(this);
        }

        public void Select()
        {
            if (gameSettingsEvent != null)
            {
                gameSettingsEvent.Raise();
            }
        }

        public void Deselect()
        {
            if (gameSettingsEvent != null)
            {
                gameSettingsEvent.Raise();
            }
        }
    }
}
