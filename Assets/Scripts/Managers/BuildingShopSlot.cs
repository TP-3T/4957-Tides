using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    private TextMeshProUGUI textField;

    public GameEvent BuildModeStartingEvent;

    public void OnClick()
    {
        BuildModeStartingEvent.Raise(feature);
    }

    public void UpdateText()
    {
        textField = GetComponentInChildren<TextMeshProUGUI>();

        textField.text = feature.DisplayName;
    }
}
