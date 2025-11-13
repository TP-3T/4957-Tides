using UnityEngine;
using TMPro;
using TTT.Managers;
using TTT.GameEvents;

public class GlobalInformation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _CO2Text;

    private GameManager GameManager;

    private int Year;
    private string Season;
    private int CO2;

    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
        setDateText();
        setCO2Text();
    }

    void Update()
    {
        if (GameManager.GetYear() != Year || GameManager.GetSeason() != Season)
        {
            setDateText();
        }
        if (GameManager.GetCO2() != CO2)
        {
            setCO2Text();
        }
    }

    private void setDateText()
    {
        Year = GameManager.GetYear();
        Season = GameManager.GetSeason();
        _dateText.text = Season + ", " + Year.ToString();
    }

    private void setCO2Text()
    {
        CO2 = GameManager.GetCO2();
        _CO2Text.text = "CO2: " + CO2.ToString() + " ppm";
    }
}
