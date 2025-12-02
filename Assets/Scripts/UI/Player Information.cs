using TMPro;
using TTT.DataClasses.States;
using UnityEngine;

/// <summary>
/// Handles displaying player information such as money, energy, and population.
/// </summary>
public class PlayerInformation : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _moneyText;

    [SerializeField]
    private TextMeshProUGUI _powerText;

    [SerializeField]
    private TextMeshProUGUI _populationText;

    [SerializeField]
    private PlayerStats playerStats;

    private void Start()
    {
        UpdateAllText();
        playerStats.ResetResources();
    }

    void Update()
    {
        UpdateAllText();
    }

    public void UpdateAllText()
    {
        if (playerStats != null)
        {
            SetMoneyText((int)playerStats.Money.AmountOwned);
            SetPowerText((int)playerStats.Power.AmountOwned);
            SetPopulationText((int)playerStats.Population.AmountOwned);
        }
    }

    private void SetMoneyText(int amount)
    {
        _moneyText.text = "Money: $" + amount.ToString();
    }

    private void SetPowerText(int amount)
    {
        _powerText.text = "Power: " + amount.ToString();
    }

    private void SetPopulationText(int amount)
    {
        _populationText.text = "Population: " + amount.ToString();
    }
}
