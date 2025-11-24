using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private TextMeshProUGUI productionText;

    [SerializeField]
    private TextMeshProUGUI energyText;

    [SerializeField]
    private Feature[] featureData;

    public GameEvent BuildModeStartingEvent;

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
        nameText = GetComponentInChildren<TextMeshProUGUI>();

        nameText.text = feature.DisplayName;
    }
}
