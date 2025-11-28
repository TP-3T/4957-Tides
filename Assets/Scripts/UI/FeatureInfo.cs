using TMPro;
using TTT.DataClasses.HexData;
using TTT.DataClasses.States;
using TTT.UI;
using UnityEngine;

public class FeatureInfo : MonoBehaviour, IOpenable
{
    /* #region IOpenable requirements */
    [field: SerializeField]
    public RectTransform ToOpen { get; set; }

    [field: SerializeField]
    public bool IsOpen { get; set; }
    public Vector2 EndPosition { get; set; }
    public Vector2 StartPosition { get; set; }

    [field: SerializeField]
    public AnimationCurve MovementCurve { get; set; }

    [field: SerializeField]
    public float MovementSeconds { get; set; }

    [field: SerializeField]
    public ShiftType ShiftDirection { get; set; }

    [field: SerializeField]
    public Vector2 ShiftPadding { get; set; }

    [field: SerializeField]
    private GameObject hexFeature;

    private TextMeshProUGUI featureTileText;
    private Coroutine CurrentShift { get; set; }

    /* #endregion*/

    [SerializeField]
    private PlayerStats playerStats;

    private void Awake()
    {
        (this as IOpenable).SetupPositions();
        if (hexFeature != null)
            {
                featureTileText = hexFeature.GetComponent<TextMeshProUGUI>();
            }
    }

    /// <summary>
    /// Cancels the current shift if one is running,
    /// then runs the movement function as per the IOpenable
    /// </summary>
    public void Toggle()
    {
        if (CurrentShift != null)
        {
            StopCoroutine(CurrentShift);
        }
        CurrentShift = StartCoroutine((this as IOpenable).ToggleOpenable());
    }

    public void OnTileSelected()
    {
        if (playerStats == null)
            return;

        var selectedCell = playerStats.selectedHexCell;

        if (selectedCell.HasValue)
        {
            if (!IsOpen)
            {
                Toggle();
            }
            UpdateTileDisplay();
        }
        else
        {
            if (IsOpen)
            {
                Toggle();
            }
        }
    }

    private void UpdateTileDisplay()
    {
        if (playerStats?.selectedHexCell == null)
            return;

        var tile = playerStats.selectedHexCell.Value;
        var tileData = playerStats.selectedTileData;

        // Format tile info for display
        string displayText = $"Position: {tile.CellPosition}\n" +
                            $"Flooded: {tile.Flooded}\n" +
                            $"Terrain: {tile.TerrainTypeId}";

        if (tileData != null)
        {
            displayText += $"\nOwner: {tileData.Owner}";
            displayText += $"\nElevation: {tileData.Elevation}";
            
            if (!string.IsNullOrEmpty(tileData.Label))
            {
                displayText += $"\nLabel: {tileData.Label}";
            }
        }

        if (featureTileText != null)
        {
            featureTileText.text = displayText;
        }
    }

    public void ClearSelection()
    {
        playerStats?.ClearSelectedTile();
    }
}
