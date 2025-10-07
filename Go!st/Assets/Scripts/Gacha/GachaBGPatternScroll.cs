using UnityEngine;

public class GachaBGPatternScrollUI : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f; // 上方向スクロール速度（ピクセル単位）
    [SerializeField] private RectTransform[] backgrounds; // ループさせる背景

    private float bgHeight;

    void Start()
    {
        if (backgrounds.Length == 0)
        {
            Debug.LogError("背景が設定されていません！");
            return;
        }

        // 背景の高さ（同じサイズを想定）
        bgHeight = backgrounds[0].rect.height;

        // 初期配置：1枚目を原点に、2枚目以降は順番に下に配置
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].localPosition = new Vector3(
                0,
                -i * bgHeight,  // 下方向に順番に配置
                backgrounds[i].localPosition.z
            );
        }
    }

    void Update()
    {
        // スクリーン上端（Canvas Pivot が中央の場合）
        float screenTopY = Screen.height / 2f;

        foreach (var bg in backgrounds)
        {
            // 上方向にスクロール
            bg.localPosition += Vector3.up * scrollSpeed * Time.deltaTime;

            // 背景がスクリーン上端を超えたら下にループ
            if (bg.localPosition.y - bgHeight / 2 > screenTopY)
            {
                // 一番下の背景の下に移動
                float minY = float.MaxValue;
                foreach (var b in backgrounds)
                {
                    if (b.localPosition.y < minY) minY = b.localPosition.y;
                }
                bg.localPosition = new Vector3(bg.localPosition.x, minY - bgHeight, bg.localPosition.z);
            }
        }
    }
}
