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
    /// rule for grid and cell size.
    public FitType fitType;

    [Min(1)]
    public int columns;

    [Min(1)]
    public int rows;

    [Space]
    [Min(0)]
    public Vector2 spacing;

    public Vector2 cellSize;

    /// If true, cell width is calculated to fit the container size.
    private bool fitX;

    /// If true, cell height is calculated to fit the container size.
    private bool fitY;

    /// <summary>
    /// Calculates and applies the grid layout to all child RectTransforms.
    /// This method handles the primary logic for cell sizing and positioning based on <see cref="fitType"/> and <see cref="alignment"/>.
    /// </summary>
    public override void CalculateLayoutInputVertical()
    {
        // 1. Determine Rows and Columns based on FitType
        CalculateGridCounts();

        // 2. Calculate Cell Size based on Alignment and Fixed Padding/Spacing
        Vector2 calculatedCellSize = CalculateCellSize();

        // 3. Apply fit flags
        cellSize.x = fitX ? Mathf.Max(0, calculatedCellSize.x) : cellSize.x;
        cellSize.y = fitY ? Mathf.Max(0, calculatedCellSize.y) : cellSize.y;

        // 4. Position all children
        SetChildPositions();
    }

    /// <summary>
    /// Calculates the amount of grids
    /// </summary>
    private void CalculateGridCounts()
    {
        float childCount = transform.childCount;
        float sqrRt = Mathf.Sqrt(childCount);
        fitX = fitY = false;

        switch (fitType)
        {
            case FitType.Uniform:
                rows = columns = Mathf.CeilToInt(sqrRt);
                columns = Mathf.CeilToInt(childCount / (float)rows);
                rows = Mathf.CeilToInt(childCount / (float)columns);
                fitX = fitY = true;
                break;
            case FitType.Width:
                rows = Mathf.CeilToInt(childCount / (float)columns);
                fitX = true;
                break;
            case FitType.Height:
                columns = Mathf.CeilToInt(childCount / (float)rows);
                fitY = true;
                break;
            case FitType.FixedRows:
                columns = Mathf.CeilToInt(childCount / (float)rows);
                break;
            case FitType.FixedColumns:
                rows = Mathf.CeilToInt(childCount / (float)columns);
                break;
            case FitType.FixedBoth:
                // Uses user-defined rows/cols, fitX/fitY remain false
                break;
        }
    }

    /// <summary>
    /// Helper method to calculate cell size.
    /// </summary>
    private Vector2 CalculateCellSize()
    {
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float totalSpaceX =
            padding.left
            + padding.right
            + (spacing.x * Mathf.Max(0, columns - 1));
        float totalSpaceY =
            padding.top + padding.bottom + (spacing.y * Mathf.Max(0, rows - 1));

        float cellWidth = (parentWidth - totalSpaceX) / (float)columns;
        float cellHeight = (parentHeight - totalSpaceY) / (float)rows;

        if (alignment == Alignment.Vertical)
        {
            // Re-calculate total fixed space based on swapped constraint counts
            totalSpaceX =
                padding.left
                + padding.right
                + (spacing.x * Mathf.Max(0, rows - 1));
            totalSpaceY =
                padding.top
                + padding.bottom
                + (spacing.y * Mathf.Max(0, columns - 1));

            // calcs cell width
            cellWidth = (parentHeight - totalSpaceY) / (float)rows;
            // calcs cell height
            cellHeight = (parentWidth - totalSpaceX) / (float)columns;
        }

        return new Vector2(cellWidth, cellHeight);
    }

    /// <summary>
    /// Helper for setting ChildPosition
    /// </summary>
    private void SetChildPositions()
    {
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        // Calculate total size consumed by the grid content
        float contentSizeX =
            (columns * cellSize.x)
            + (spacing.x * Mathf.Max(0, columns - 1))
            + padding.left
            + padding.right;
        float contentSizeY =
            (rows * cellSize.y)
            + (spacing.y * Mathf.Max(0, rows - 1))
            + padding.top
            + padding.bottom;

        float offsetX = 0f;
        float offsetY = 0f;

        // Horizontal Alignment
        if (((int)m_ChildAlignment % 3) == 1) // Center (1, 4, 7)
        {
            offsetX = (parentWidth - contentSizeX) * 0.5f;
        }
        else if (((int)m_ChildAlignment % 3) == 2) // Right (2, 5, 8)
        {
            offsetX = parentWidth - contentSizeX;
        }

        // Vertical Alignment
        if (((int)m_ChildAlignment / 3) == 1) // Middle (3, 4, 5)
        {
            offsetY = (parentHeight - contentSizeY) * 0.5f;
        }
        else if (((int)m_ChildAlignment / 3) == 2) // Lower (6, 7, 8)
        {
            offsetY = parentHeight - contentSizeY;
        }

        // Position children now.
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform item = rectChildren[i];

            int columnIdx,
                rowIdx;

            if (alignment == Alignment.Horizontal)
            {
                rowIdx = i / columns;
                columnIdx = i % columns;
            }
            else // Alignment.Vertical
            {
                // In vertical mode, items flow down first, so rows=primary, columns=secondary
                columnIdx = i / rows;
                rowIdx = i % rows;
            }

            float xPos =
                padding.left + (cellSize.x + spacing.x) * columnIdx + offsetX;
            float yPos =
                padding.top + (cellSize.y + spacing.y) * rowIdx + offsetY;
            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }

    /// <summary>Required by <see cref="LayoutGroup"/> but not implemented as layout is calculated in <see cref="CalculateLayoutInputVertical"/>.</summary>
    public override void SetLayoutHorizontal() { }

    /// <summary>Required by <see cref="LayoutGroup"/> but not implemented as layout is calculated in <see cref="CalculateLayoutInputVertical"/>.</summary>
    public override void SetLayoutVertical() { }
}
