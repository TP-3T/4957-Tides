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

    private Coroutine CurrentShift {get; set;}

    //Need to add scriptable object that reads to display currently selected cell info. And parse out the json data to presentable format
    //Need to create scriptable object that holds the tile data of selected


    /* #endregion*/
    private void Awake()
    {
        (this as IOpenable).SetupPositions();
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
    ///<summary>
    /// Helper function when a tile is clicked
    /// to open the feature info panel.
    /// Will need to connect this with tile clicked event
    /// </summary>
    public void OnTileClicked()
    {
        if (!IsOpen)
        {
            Toggle();
        }
        //Need to add a clear selected that toggles this back to closed but will add later
    }
}       
