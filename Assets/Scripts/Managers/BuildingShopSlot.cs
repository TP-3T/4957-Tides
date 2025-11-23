using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    [SerializeField]
    private TextMeshProUGUI textField;

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
        textField = GetComponentInChildren<TextMeshProUGUI>();

        textField.text = feature.DisplayName;
    }
}
