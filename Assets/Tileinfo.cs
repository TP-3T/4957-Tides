using TTT.UI;
using UnityEngine;

public class Tileinfo : MonoBehaviour, IOpenable
{
    /* #region IOpenable requirements */
    [field: SerializeField]
    public RectTransform ToHide { get; set; }

    [field: SerializeField]
    public bool IsOpen { get; set; }
    public Vector2 OpenPosition { get; set; }
    public Vector2 ClosedPosition { get; set; }

    [field: SerializeField]
    public AnimationCurve MovementCurve { get; set; }

    [field: SerializeField]
    public float MovementSeconds { get; set; }

    [field: SerializeField]
    public ShiftType ShiftDirection { get; set; }

    [field: SerializeField]
    public Vector2 ShiftPadding { get; set; }

    /* #endregion*/
    void Awake()
    {
        (this as IOpenable).SetupPositions();
    }
}
