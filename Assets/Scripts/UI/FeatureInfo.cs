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

    private Coroutine CurrentShift { get; set; }

    /* #endregion*/

    [SerializeField]
    private PlayerStats playerStats;

    private void Awake()
    {
        (this as IOpenable).SetupPositions();
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnTileSelected.AddListener(OnTileSelected);
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnTileSelected.RemoveListener(OnTileSelected);
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

        var selectedCell = playerStats.SelectedHexCell;

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
        if (playerStats?.SelectedHexCell == null)
            return;

        var tile = playerStats.SelectedHexCell.Value;
        var tileData = playerStats.SelectedTileData;

        // TODO: Update UI elements with tile data
        Debug.Log(
            $"Selected tile at position: {tile.CellPosition}, flooded: {tile.Flooded}, terrain: {tile.TerrainTypeId}"
        );

        if (tileData != null)
        {
            Debug.Log(
                $"Tile data - Feature: {tileData.Feature}, Owner: {tileData.Owner}"
            );
        }
    }

    public void ClearSelection()
    {
        playerStats?.ClearSelectedTile();
    }
}
