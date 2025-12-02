using TMPro;
using TTT.DataClasses.States;
using TTT.Managers;
using UnityEngine;

public class GlobalInformation : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _dateText;

    [SerializeField]
    private TextMeshProUGUI _CO2Text;

    [SerializeField]
    private PlayerStats playerStats;


    [Tooltip("Multiplier to convert pollution units to ppm for display")]
    [SerializeField]
    private float pollutionToPpmMultiplier = 0.001f;

    private GameManager GameManager;

    private int Year;
    private string Season;
    private int CO2;

    void Start()
    {
        GameManager = GameManager.Instance;
        setDateText();
        setCO2Text();
    }

    void Update()
    {
        if (    GameManager.GlobalInformation.Value.Year != Year
            ||  GameManager.GlobalInformation.Value.Season.ToString() != Season)
        {
            setDateText();
        }
        if (playerStats != null && playerStats.pollution != null && playerStats.pollution.AmountOwned != CO2)
        {
            setCO2Text();
        }
    }

    private void setDateText()
    {
        Year = GameManager.GlobalInformation.Value.Year;
        Season = GameManager.GlobalInformation.Value.Season.ToString();
        _dateText.text = $"{Season}, {Year}";
    }

    private void setCO2Text()
    {
        if (playerStats != null && playerStats.pollution != null)
        {
            CO2 = playerStats.pollution.AmountOwned;
            float displayPpm = CO2 * pollutionToPpmMultiplier;
            _CO2Text.text = $"CO2: {displayPpm:F1} ppm";
        }
        else
        {
            _CO2Text.text = "CO2: -- ppm";
        }
    }
}
