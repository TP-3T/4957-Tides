using TMPro;
using TTT.Managers;
using UnityEngine;

public class GlobalInformation : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _dateText;

    [SerializeField]
    private TextMeshProUGUI _CO2Text;

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
        if (GameManager.Year != Year || GameManager.Season != Season)
        {
            setDateText();
        }
        if (GameManager.CO2 != CO2)
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
        CO2 = GameManager.CO2;
        _CO2Text.text = $"CO2: {CO2} ppm";
    }
}
