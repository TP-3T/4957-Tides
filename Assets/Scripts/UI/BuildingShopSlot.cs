using TMPro;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.UI;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    [SerializeField]
    private TextMeshProUGUI featureNameText;

    [SerializeField]
    private Image featureIcon;

    [SerializeField]
    private Image moneyRevIcon;

    [SerializeField]
    private TextMeshProUGUI moneyRevText;

    [SerializeField]
    private Image pollRevIcon;

    [SerializeField]
    private TextMeshProUGUI pollRevText;

    [SerializeField]
    private Image moneyCostIcon;

    [SerializeField]
    private TextMeshProUGUI moneyCostText;

    [SerializeField]
    private Image energyCostIcon;

    [SerializeField]
    private TextMeshProUGUI energyCostText;

    [SerializeField]
    private Image popCostIcon;

    [SerializeField]
    private TextMeshProUGUI popCostText;

    [SerializeField]
    private Image hexConstraintIcon;

    [SerializeField]
    private HorizontalLayoutGroup hexConstraintHexContainer;

    public GameEvent InteractModeChange;

    void Start()
    {
        UpdateText();
    }

    public void OnClick()
    {
        InteractModeChange.Raise(
            new InteractionModeChangeEventArgs()
            {
                NewMode = InteractionMode.BUILDING,
            }
        );
    }

    public void UpdateText()
    {
        // Null safety
        if (feature == null)
            return;

        // Set feature name
        if (featureNameText != null)
            featureNameText.text = feature.DisplayName;

        // Accumulators for each resource type
        int totalMoneyRevenue = 0;
        int totalPollutionRevenue = 0;
        int totalMoneyCost = 0;
        int totalEnergyCost = 0;
        int totalPopulationCost = 0;

        // Process all resource producers
        foreach (var producer in feature.ResourceProducers)
        {
            foreach (var amount in producer.ResourceAmounts)
            {
                int value = amount.Count;

                switch (amount.Thing.Name)
                {
                    case "money":
                        totalMoneyRevenue += value;
                        break;
                    case "pollution":
                        totalPollutionRevenue += value;
                        break;
                }
            }
        }

        // Process costs
        foreach (var cost in feature.Cost)
        {
            int value = cost.Count;

            switch (cost.Thing.Name)
            {
                case "money":
                    totalMoneyCost += value;
                    break;
                case "power":
                    totalEnergyCost += value;
                    break;
                case "population":
                    totalPopulationCost += value;
                    break;
            }
        }

        //This should be a resource producer IMO, but I'm not changing it now
        if (feature.PollutionEmission > 0)
            totalPollutionRevenue += feature.PollutionEmission;

        //Manage constraint icons
        if (
            feature.Constraints != null
            && feature.Constraints.TerrainConstraints != null
        )
        {
            // Clear existing icons
            foreach (Transform child in hexConstraintHexContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Add new icons based on terrain constraints
            foreach (var terrain in feature.Constraints.TerrainConstraints.List)
            { }
        }

        // Display accumulated values
        SetMoneyRevDisplay(totalMoneyRevenue);
        SetPollutionDisplay(totalPollutionRevenue);
        SetEnergyDisplay(totalEnergyCost);
        SetPopulationDisplay(totalPopulationCost);
    }

    private void SetMoneyRevDisplay(int revenue)
    {
        if (revenue > 0 && moneyRevText != null)
        {
            moneyRevText.text = revenue.ToString();
        }
    }

    private void SetPollutionDisplay(int revenue)
    {
        if (revenue > 0 && pollRevText != null)
        {
            pollRevText.text = revenue.ToString();
        }
    }

    private void SetEnergyDisplay(int cost)
    {
        if (cost > 0 && energyCostIcon != null && energyCostText != null)
        {
            energyCostText.text = cost.ToString();
        }
    }

    private void SetPopulationDisplay(int cost)
    {
        if (cost > 0 && popCostIcon != null && popCostText != null)
        {
            popCostText.text = cost.ToString();
        }
    }
}
