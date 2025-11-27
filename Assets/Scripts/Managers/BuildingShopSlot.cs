using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    [SerializeField]
    private TextMeshProUGUI featureNameText;

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
        featureNameText = GetComponentInChildren<TextMeshProUGUI>();

        featureNameText.text = feature.DisplayName;
    }
}
