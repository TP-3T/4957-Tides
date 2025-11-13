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

    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
        Season = GameManager.GetSeason();
        Year = GameManager.GetYear();
        _dateText.text = Season + ", " + Year.ToString();
    }

    void Update()
    {
        if (GameManager.GetYear() == Year && GameManager.GetSeason() == Season)
        {
            return;
        }
        Year = GameManager.GetYear();
        Season = GameManager.GetSeason();
        _dateText.text = Season + ", " + Year.ToString();
    }
}
