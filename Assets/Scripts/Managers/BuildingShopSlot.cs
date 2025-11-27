using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    private Sprite hexConstraintIcon;

    [SerializeField]
    private Sprite hexConstraintRestrictedIcon;

    [SerializeField]
    private HorizontalLayoutGroup hexConstraintHexContainer;

    public GameEvent BuildModeStartingEvent;

    private const int IconSize = 50;

    void Start()
    {
        UpdateText();
    }

    public void OnClick()
    {
        BuildModeStartingEvent.Raise(feature);
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

            TTT.DataClasses.FilterListMode mode = feature
                .Constraints
                .TerrainConstraints
                .Mode;

            Sprite iconToUse = null;

            if (mode == TTT.DataClasses.FilterListMode.WHITELIST)
            {
                iconToUse = hexConstraintIcon;
            }
            else if (mode == TTT.DataClasses.FilterListMode.BLACKLIST)
            {
                iconToUse = hexConstraintRestrictedIcon;
            }

            // Add icons based on terrain constraints
            if (iconToUse != null)
            {
                foreach (
                    var terrain in feature.Constraints.TerrainConstraints.List
                )
                {
                    if (mode == TTT.DataClasses.FilterListMode.WHITELIST)
                    {
                        // Single-layer icon
                        GameObject containerObj = new GameObject("TerrainIcon");
                        containerObj.transform.SetParent(
                            hexConstraintHexContainer.transform,
                            false
                        );
                        LayoutElement iconLayout =
                            containerObj.AddComponent<LayoutElement>();
                        iconLayout.preferredWidth = IconSize;
                        iconLayout.preferredHeight = IconSize;

                        GameObject iconObj = new GameObject("Icon");
                        iconObj.transform.SetParent(
                            containerObj.transform,
                            false
                        );
                        RectTransform iconRect =
                            iconObj.AddComponent<RectTransform>();
                        iconRect.anchorMin = Vector2.zero;
                        iconRect.anchorMax = Vector2.one;
                        iconRect.offsetMin = Vector2.zero;
                        iconRect.offsetMax = Vector2.zero;
                        Image iconImage = iconObj.AddComponent<Image>();
                        iconImage.sprite = iconToUse;
                        iconImage.preserveAspect = true;
                        iconImage.color = terrain.Color;
                    }
                    else if (mode == TTT.DataClasses.FilterListMode.BLACKLIST)
                    {
                        // Layered icon with overlay
                        CreateLayeredTerrainIcon(
                            terrain,
                            hexConstraintIcon,
                            hexConstraintRestrictedIcon,
                            hexConstraintHexContainer.transform
                        );
                    }
                }
            }
        }

        // Display accumulated values
        SetMoneyRevDisplay(totalMoneyRevenue);
        SetPollutionDisplay(totalPollutionRevenue);
        SetEnergyDisplay(totalEnergyCost);
        SetPopulationDisplay(totalPopulationCost);
    }

    private GameObject CreateLayeredTerrainIcon(
        TTT.DataClasses.Terrain.TerrainType terrain,
        Sprite baseSprite,
        Sprite overlaySprite,
        Transform parentContainer
    )
    {
        // Create container
        GameObject containerObj = new GameObject("TerrainIconContainer");
        containerObj.transform.SetParent(parentContainer, false);

        // Add LayoutElement for HorizontalLayoutGroup sizing
        LayoutElement layoutElement =
            containerObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = IconSize;
        layoutElement.preferredHeight = IconSize;

        // Create base layer (colored hexagon)
        GameObject baseLayer = new GameObject("BaseIcon");
        baseLayer.transform.SetParent(containerObj.transform, false);
        RectTransform baseRect = baseLayer.AddComponent<RectTransform>();
        baseRect.anchorMin = Vector2.zero;
        baseRect.anchorMax = Vector2.one;
        baseRect.offsetMin = Vector2.zero;
        baseRect.offsetMax = Vector2.zero;
        Image baseImage = baseLayer.AddComponent<Image>();
        baseImage.sprite = baseSprite;
        baseImage.preserveAspect = true;
        baseImage.color = terrain.Color;

        // Create overlay layer (restriction symbol)
        GameObject overlayLayer = new GameObject("OverlayIcon");
        overlayLayer.transform.SetParent(containerObj.transform, false);
        RectTransform overlayRect = overlayLayer.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlayImage = overlayLayer.AddComponent<Image>();
        overlayImage.sprite = overlaySprite;
        overlayImage.preserveAspect = true;
        overlayImage.color = Color.white;

        return containerObj; // po: why does this return if we never use the return value
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
