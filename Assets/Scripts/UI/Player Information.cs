using TMPro;
using TTT.DataClasses.PlayerResources;
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
    private PlayerResource money;

    [SerializeField]
    private PlayerResource power;

    [SerializeField]
    private PlayerResource population;

    private int cachedMoneyAmount;

    private int cachedPowerAmount;

    private int cachedPopulationAmount;

    /// <summary>
    /// Updates the player information display each frame.
    /// </summary>
    void Update()
    {
        if (cachedMoneyAmount != money.AmountOwned)
        {
            cachedMoneyAmount = money.AmountOwned;
            setMoneyText();
        }
        if (cachedPowerAmount != power.AmountOwned)
        {
            cachedPowerAmount = power.AmountOwned;
            setPowerText();
        }
        if (cachedPopulationAmount != population.AmountOwned)
        {
            cachedPopulationAmount = population.AmountOwned;
            setPopulationText();
        }
    }

    /// <summary>
    /// Updates the money display text.
    /// </summary>
    private void setMoneyText()
    {
        _moneyText.text = "Money: $" + money.AmountOwned.ToString();
    }

    /// <summary>
    /// Updates the power display text.
    /// </summary>
    private void setPowerText()
    {
        _powerText.text = "Power: " + power.AmountOwned.ToString();
    }

    /// <summary>
    /// Updates the population display text.
    /// </summary>
    private void setPopulationText()
    {
        _populationText.text =
            "Population: " + population.AmountOwned.ToString();
    }
}
