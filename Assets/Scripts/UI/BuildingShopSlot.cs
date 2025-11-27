using TMPro;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using UnityEngine;

public class BuildingShopSlot : MonoBehaviour
{
    public FeatureType feature;

    [SerializeField]
    private TextMeshProUGUI textField;

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
        textField = GetComponentInChildren<TextMeshProUGUI>();

        textField.text = feature.DisplayName;
    }
}
