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

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnResourcesChanged.AddListener(UpdateAllText);
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnResourcesChanged.RemoveListener(UpdateAllText);
        }
    }

    private void Start()
    {
        UpdateAllText();
    }

    public void UpdateAllText()
    {
        if (playerStats != null)
        {
            
            SetMoneyText(playerStats.money?.AmountOwned ?? 0);
            SetPowerText(playerStats.power?.AmountOwned ?? 0);
            SetPopulationText(playerStats.population?.AmountOwned ?? 0);
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
