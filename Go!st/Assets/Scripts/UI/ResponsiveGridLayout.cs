using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    [SerializeField] int columnCount = 4;        // 1行に並べたい数
    [SerializeField] float spacing = 10f;        // GridLayoutGroupのspacingと一致させておく
    [SerializeField] float padding = 20f;        // 左右のパディング（両端合計）

    private GridLayoutGroup grid;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
        float totalSpacing = spacing * (columnCount - 1);
        float totalPadding = padding;
        float parentWidth = ((RectTransform)transform).rect.width;

        float cellWidth = (parentWidth - totalSpacing - totalPadding) / columnCount;

        grid.cellSize = new Vector2(cellWidth, cellWidth); // 正方形セルの場合
    }

    // サイズが変わるタイミングでも再計算したい場合
    void OnRectTransformDimensionsChange()
    {
        if (grid != null)
            UpdateCellSize();
    }
}
