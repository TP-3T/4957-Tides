using UnityEngine;
using TMPro;
using TTT.Managers;
using TTT.GameEvents;

public class GlobalInformation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _CO2Text;

    [SerializeField] private GameManager GameManager;

    private int Year;
    private string Season;

    void Start()
    {
        Season = GameManager.GetSeason();
        Year = GameManager.GetYear();

        Debug.Log(GameManager.GetSeason());

        _dateText.text = Season + ", " + Year.ToString();
    }

    void Update()
    {
        if (GameManager.GetYear() == Year && GameManager.GetSeason() == Season)
        {
            return;
        }
        Debug.Log(GameManager.GetSeason());
        Year = GameManager.GetYear();
        Season = GameManager.GetSeason();
        _dateText.text = Season + ", " + Year.ToString();
    }
}
