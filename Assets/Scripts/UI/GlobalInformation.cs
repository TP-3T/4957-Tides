using TMPro;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Managers;
using Unity.Netcode;
using UnityEngine;

namespace TTT.UI
{
    public class GlobalInformation : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _dateText;

        [SerializeField]
        private TextMeshProUGUI _CO2Text;

        [SerializeField]
        private PlayerStats playerStats;

        void Start()
        {
            // setDateText();
            GameManager.Instance.CO2_Pollution.OnValueChanged += SetCO2Text;
            SetCO2Text(0f, GameManager.Instance.CO2_Pollution.Value);
        }

        private void SetCO2Text(float _, float newValue)
        {
            _CO2Text.text = $"CO2: {newValue:F1} ppm";
        }

        public void OnTurnEnded(Object args)
        {
            Debug.Log(
                $"[GlobalInformation] TURN WAS ENDED {NetworkManager.Singleton.LocalClientId}"
            );
            if (args is not EndTurnEventArgs evArgs)
            {
                Debug.LogWarning(
                    "Turn ended args should be type EndTurnEventArgs"
                );
                return;
            }
            _dateText.text = $"{evArgs.Season}, {evArgs.Year}";
        }
    }
}
