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
                || GameManager.Instance.Season != Season
            )
            {
                setDateText();
            }
        }

        private void setDateText()
        {
            Year = GameManager.Instance.Year;
            Season = GameManager.Instance.Season;
            _dateText.text = $"{Season}, {Year}";
        }

        private void SetCO2Text(float _, float newValue)
        {
            _CO2Text.text = $"CO2: {newValue:F1} ppm";
        }
    }
}

// void Update()
// {
//     if (GameManager.Year != Year || GameManager.Season != Season)
//     {
//         setDateText();
//     }
//     if (GameManager.CO2_Pollution != null
//     // && playerStats.pollution.AmountOwned != CO2
//     )
//     {
//         setCO2Text();
//     }
// }

// private void setDateText()
// {
//     Year = GameManager.Year;
//     Season = GameManager.Season;
//     _dateText.text = $"{Season}, {Year}";
// }

// private void setCO2Text(float old, float new)
// {
//     _CO2Text.text = $"CO2: {new:F1} ppm";
//     if (GameManager.CO2_Pollution != null)
//     {
//         // CO2 = GameManager.CO2_Pollution.Value;
//         // float displayPpm = CO2;
//         _CO2Text.text = $"CO2: {GameManager.CO2_Pollution.Value:F1} ppm";
//     }
//     else
//     {
//         _CO2Text.text = "CO2: -- ppm";
//     }
// }
