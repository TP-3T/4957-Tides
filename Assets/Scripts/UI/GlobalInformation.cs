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

    // [Tooltip("Multiplier to convert pollution units to ppm for display")]
    // [SerializeField]
    // private float pollutionToPpmMultiplier = 0.001f;

    private GameManager GameManager;

    private int Year;
    private string Season;
    private float CO2;

    void Start()
    {
        GameManager = GameManager.Instance;
        setDateText();
        setCO2Text();
    }

    void Update()
    {
        if (GameManager.Year != Year || GameManager.Season != Season)
        {
            setDateText();
        }
        if (GameManager.CO2_Pollution != null
        // && playerStats.pollution.AmountOwned != CO2
        )
        {
            setCO2Text();
        }
    }

    private void setDateText()
    {
        Year = GameManager.Year;
        Season = GameManager.Season;
        _dateText.text = $"{Season}, {Year}";
    }

    private void setCO2Text()
    {
        if (GameManager.CO2_Pollution != null)
        {
            CO2 = GameManager.CO2_Pollution.Value;
            float displayPpm = CO2;
            _CO2Text.text = $"CO2: {displayPpm:F1} ppm";
        }
        else
        {
            _CO2Text.text = "CO2: -- ppm";
        }
    }
}
