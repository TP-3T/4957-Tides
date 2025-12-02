using System;
using TMPro;
using TTT.DataClasses.States;
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

        private int Year;
        private string Season;
        private float CO2;

        void Start()
        {
            // setDateText();
            GameManager.Instance.CO2_Pollution.OnValueChanged += SetCO2Text;
            SetCO2Text(0f, GameManager.Instance.CO2_Pollution.Value);
        }

        void Update()
        {
            if (
                GameManager.Instance.Year != Year
                || GameManager.Instance.Season.ToString() != Season
            )
            {
                SetDateText();
            }
        }

        private void SetDateText()
        {
            Year = GameManager.Instance.Year;
            Season = GameManager.Instance.Season.ToString();
            _dateText.text = $"{Season}, {Year}";
        }

        private void SetCO2Text(float _, float newValue)
        {
            _CO2Text.text = $"CO2: {newValue:F1} ppm";
        }
    }
}
