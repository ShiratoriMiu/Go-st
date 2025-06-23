using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    [SerializeField] int columnCount = 4;        // 1�s�ɕ��ׂ�����
    [SerializeField] float spacing = 10f;        // GridLayoutGroup��spacing�ƈ�v�����Ă���
    [SerializeField] float padding = 20f;        // ���E�̃p�f�B���O�i���[���v�j

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

        grid.cellSize = new Vector2(cellWidth, cellWidth); // �����`�Z���̏ꍇ
    }

    // �T�C�Y���ς��^�C�~���O�ł��Čv�Z�������ꍇ
    void OnRectTransformDimensionsChange()
    {
        if (grid != null)
            UpdateCellSize();
    }
}
