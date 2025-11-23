using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A custom <see cref="LayoutGroup"/> for Unity UI that arranges child elements
/// in a grid with dynamic resizing based on various fit types.
/// <authors Richard, Alfredo>
/// </summary>
public class FlexibleGridLayout : LayoutGroup
{
    public enum Alignment
    {
        Horizontal,

        Vertical,
    }

    public enum FitType
    {
        Uniform,

        Width,

        Height,

        FixedRows,

        FixedColumns,

        FixedBoth,
    }

    public Alignment alignment;

    [Space]
    /// <summary>The rule to determine the grid structure and cell size.</summary>
    public FitType fitType;

    [Min(1)]
    public int columns;

    [Min(1)]
    public int rows;

    [Space]
    [Min(0)]
    public Vector2 spacing;

    /// <summary>The calculated size of each cell in the grid.</summary>
    public Vector2 cellSize;

    /// <summary>If true, cell width is calculated to fit the container size.</summary>
    public bool fitX;

    /// <summary>If true, cell height is calculated to fit the container size.</summary>
    public bool fitY;

    /// <summary> If true, centers the last item in the last partial row/column
    public bool NudgeLastItemsOver;

    /// <summary>
    /// Calculates and applies the grid layout to all child RectTransforms.
    /// This method handles the primary logic for cell sizing and positioning based on <see cref="fitType"/> and <see cref="alignment"/>.
    /// </summary>
    public override void CalculateLayoutInputVertical()
    {
        base.CalculateLayoutInputHorizontal();

        float sqrRt;

        switch (fitType)
        {
            case FitType.Uniform:

            default:

                fitX = fitY = true;

                sqrRt = Mathf.Sqrt(transform.childCount);

                rows = Mathf.CeilToInt(sqrRt);

                columns = Mathf.CeilToInt(sqrRt);

                rows = Mathf.CeilToInt(transform.childCount / (float)columns);

                columns = Mathf.CeilToInt(transform.childCount / (float)rows);

                break;

            case FitType.Width:

                fitX = fitY = true;

                sqrRt = Mathf.Sqrt(transform.childCount);

                rows = Mathf.CeilToInt(sqrRt);

                columns = Mathf.CeilToInt(sqrRt);

                rows = Mathf.CeilToInt(transform.childCount / (float)columns);

                break;

            case FitType.Height:

                fitX = fitY = true;

                sqrRt = Mathf.Sqrt(transform.childCount);

                rows = Mathf.CeilToInt(sqrRt);

                columns = Mathf.CeilToInt(sqrRt);

                columns = Mathf.CeilToInt(transform.childCount / (float)rows);

                break;

            case FitType.FixedRows:

                fitX = fitY = false;

                columns = Mathf.CeilToInt(transform.childCount / (float)rows);

                break;

            case FitType.FixedColumns:

                fitX = fitY = false;

                rows = Mathf.CeilToInt(transform.childCount / (float)columns);

                break;

            case FitType.FixedBoth:

                fitX = fitY = false;

                break;
        }

        float cellWidth;

        float cellHeight;

        switch (alignment)
        {
            case Alignment.Horizontal:

                cellWidth =
                    (this.rectTransform.rect.width / (float)columns)
                    - ((spacing.x / (float)columns) * (columns - 1))
                    - (padding.left / (float)columns)
                    - (padding.right / (float)columns);

                cellHeight =
                    (this.rectTransform.rect.height / (float)rows)
                    - ((spacing.y / (float)rows) * (rows - 1))
                    - (padding.top / (float)rows)
                    - (padding.bottom / (float)rows);

                break;

            case Alignment.Vertical:

            default:

                cellHeight =
                    (this.rectTransform.rect.width / (float)columns)
                    - ((spacing.x / (float)columns) * (columns - 1))
                    - (padding.left / (float)columns)
                    - (padding.right / (float)columns);

                cellWidth =
                    (this.rectTransform.rect.height / (float)rows)
                    - ((spacing.y / (float)rows) * (rows - 1))
                    - (padding.top / (float)rows)
                    - (padding.bottom / (float)rows);

                break;
        }

        cellSize.x = fitX
            ? (cellWidth <= 0 ? cellSize.x : cellWidth)
            : cellSize.x;

        cellSize.y = fitY
            ? (cellHeight <= 0 ? cellSize.y : cellHeight)
            : cellSize.y;

        int columnCount = 0;

        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            var item = rectChildren[i];

            float xPos;

            float yPos;

            float xLastItemOffset = 0;

            switch (alignment)
            {
                case Alignment.Horizontal:

                    rowCount = i / columns;

                    columnCount = i % columns;

                    if (
                        NudgeLastItemsOver
                        && rowCount == (rectChildren.Count / columns)
                    )
                    {
                        xLastItemOffset = (cellSize.x + padding.left) / 2;
                    }

                    break;

                case Alignment.Vertical:

                default:

                    rowCount = i / rows;

                    columnCount = i % rows;

                    if (
                        NudgeLastItemsOver
                        && rowCount == (rectChildren.Count / rows)
                    )
                    {
                        xLastItemOffset = (cellSize.x + padding.left) / 2;
                    }

                    break;
            }

            xPos =
                (cellSize.x * columnCount)
                + (spacing.x * columnCount)
                + padding.left
                + xLastItemOffset;

            yPos =
                (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            switch (m_ChildAlignment)
            {
                case TextAnchor.UpperLeft:

                default:

                    //No need to change xPos;

                    //No need to change yPos;

                    break;

                case TextAnchor.UpperCenter:

                    xPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                            + (spacing.x + padding.left + padding.left)
                            - (
                                columns
                                * (cellSize.x + spacing.x + padding.left)
                            )
                        )
                    ); //Center xPos

                    //No need to change yPos;

                    break;

                case TextAnchor.UpperRight:

                    xPos =
                        -xPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                        - cellSize.x; //Flip xPos to go bottom-up

                    //No need to change yPos;

                    break;

                case TextAnchor.MiddleLeft:

                    //No need to change xPos;

                    yPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                            + (spacing.y + padding.top + padding.top)
                            - (rows * (cellSize.y + spacing.y + padding.top))
                        )
                    ); //Center yPos

                    break;

                case TextAnchor.MiddleCenter:

                    xPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                            + (spacing.x + padding.left + padding.left)
                            - (
                                columns
                                * (cellSize.x + spacing.x + padding.left)
                            )
                        )
                    ); //Center xPos

                    yPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                            + (spacing.y + padding.top + padding.top)
                            - (rows * (cellSize.y + spacing.y + padding.top))
                        )
                    ); //Center yPos

                    break;

                case TextAnchor.MiddleRight:

                    xPos =
                        -xPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                        - cellSize.x; //Flip xPos to go bottom-up

                    yPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                            + (spacing.y + padding.top + padding.top)
                            - (rows * (cellSize.y + spacing.y + padding.top))
                        )
                    ); //Center yPos

                    break;

                case TextAnchor.LowerLeft:

                    //No need to change xPos;

                    yPos =
                        -yPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                        - cellSize.y; //Flip yPos to go Right to Left

                    break;

                case TextAnchor.LowerCenter:

                    xPos += (
                        0.5f
                        * (
                            this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                            + (spacing.x + padding.left + padding.left)
                            - (
                                columns
                                * (cellSize.x + spacing.x + padding.left)
                            )
                        )
                    ); //Center xPos

                    yPos =
                        -yPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                        - cellSize.y; //Flip yPos to go Right to Left

                    break;

                case TextAnchor.LowerRight:

                    xPos =
                        -xPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.x
                        - cellSize.x; //Flip xPos to go bottom-up

                    yPos =
                        -yPos
                        + this.gameObject.GetComponent<RectTransform>().sizeDelta.y
                        - cellSize.y; //Flip yPos to go Right to Left

                    break;
            }

            SetChildAlongAxis(item, 0, xPos, cellSize.x);

            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }

    /// <summary>Required by <see cref="LayoutGroup"/> but not implemented as layout is calculated in <see cref="CalculateLayoutInputVertical"/>.</summary>
    public override void SetLayoutHorizontal() { }

    /// <summary>Required by <see cref="LayoutGroup"/> but not implemented as layout is calculated in <see cref="CalculateLayoutInputVertical"/>.</summary>
    public override void SetLayoutVertical() { }
}
