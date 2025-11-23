using System.Runtime.Serialization;
using TMPro;
using TTT.DataClasses.PlayerResources;
using TTT.GameEvents;
using TTT.Managers;
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

    /// <summary>
    /// Initializes the player information display.
    /// </summary>
    void Start()
    {
        setMoneyText();
        setPowerText();
        setPopulationText();
    }

    /// <summary>
    /// Updates the player information display each frame.
    /// </summary>
    void Update() { }

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
