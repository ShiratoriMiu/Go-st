using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUIAdjuster : MonoBehaviour
{
    [Header("調整対象（UI設定画面用）")]
    [SerializeField] private RectTransform[] adjustmentImages;

    [Header("実際のプレイで使う画像")]
    [SerializeField] private RectTransform[] skillButtonImages;

    [Header("UI要素")]
    [SerializeField] private CustomSlider moveSlider;   // 上下移動用
    [SerializeField] private CustomSlider sizeSlider;   // サイズ調整用

    [Header("設定値")]
    [SerializeField, Range(0f, 1f)] private float defaultMoveValue = 0.15f;
    [SerializeField, Range(0f, 1f)] private float defaultSizeValue = 0.5f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;

    private Canvas canvas;
    private RectTransform canvasRect;

    private const string MOVE_KEY = "ImageMoveSliderValue";
    private const string SIZE_KEY = "ImageSizeSliderValue";

    private void Start()
    {
        if (adjustmentImages.Length == 0)
        {
            Debug.LogWarning("⚠️ adjustmentImages が設定されていません。");
            return;
        }

        canvas = adjustmentImages[0].GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // 🔹 保存値をロード
        float savedMoveValue = PlayerPrefs.GetFloat(MOVE_KEY, defaultMoveValue);
        float savedSizeValue = PlayerPrefs.GetFloat(SIZE_KEY, defaultSizeValue);

        moveSlider.value = savedMoveValue;
        sizeSlider.value = savedSizeValue;

        moveSlider.onValueChanged.AddListener(UpdateImagesTransform);
        sizeSlider.onValueChanged.AddListener(UpdateImagesTransform);

        // 🔹 初期反映
        UpdateImagesTransform(savedMoveValue);

        // 🔹 実プレイ用ボタンにも反映（起動時から適用されるように）
        ApplyToSkillButtons(savedMoveValue, savedSizeValue);
    }

    /// <summary>
    /// スライダー変更時に画像の位置・サイズを更新
    /// </summary>
    private void UpdateImagesTransform(float _)
    {
        if (canvasRect == null) return;

        float moveValue = moveSlider.value;
        float sizeValue = sizeSlider.value;
        float canvasHeight = canvasRect.rect.height;

        foreach (var target in adjustmentImages)
        {
            if (target == null) continue;

            // 🔸 もともとの符号（Xが-1なら反転を維持）
            float signX = Mathf.Sign(target.localScale.x);

            // 🔸 スケール計算（反転維持）
            float scale = Mathf.Lerp(minScale, maxScale, sizeValue);
            target.localScale = new Vector3(signX * scale, scale, 1f);

            // 🔸 高さ再計算
            float imageHeight = target.rect.height * scale;

            // 🔸 上端が中央を超えないように
            float minY = 0f;
            float maxY = (canvasHeight / 2f) - imageHeight;

            float newY = Mathf.Lerp(minY, maxY, moveValue);
            newY = Mathf.Min(newY, maxY);

            Vector2 anchoredPos = target.anchoredPosition;
            anchoredPos.y = newY;
            target.anchoredPosition = anchoredPos;
        }
    }

    /// <summary>
    /// 保存ボタンから呼ぶ
    /// </summary>
    public void SavePosition()
    {
        float moveValue = moveSlider.value;
        float sizeValue = sizeSlider.value;

        PlayerPrefs.SetFloat(MOVE_KEY, moveValue);
        PlayerPrefs.SetFloat(SIZE_KEY, sizeValue);
        PlayerPrefs.Save();

        Debug.Log($"位置データを保存しました: Move={moveValue}, Size={sizeValue}");

        ApplyToSkillButtons(moveValue, sizeValue);
    }

    /// <summary>
    /// 実プレイ用ボタンに反映
    /// </summary>
    private void ApplyToSkillButtons(float moveValue, float sizeValue)
    {
        if (canvasRect == null) return;

        float canvasHeight = canvasRect.rect.height;
        float scale = Mathf.Lerp(minScale, maxScale, sizeValue);

        foreach (var target in skillButtonImages)
        {
            if (target == null) continue;

            // 🔸 符号を維持してスケール適用
            float signX = Mathf.Sign(target.localScale.x);
            target.localScale = new Vector3(signX * scale, scale, 1f);

            float imageHeight = target.rect.height * scale;
            float minY = 0f;
            float maxY = (canvasHeight / 2f) - imageHeight;
            float newY = Mathf.Lerp(minY, maxY, moveValue);
            newY = Mathf.Min(newY, maxY);

            Vector2 anchoredPos = target.anchoredPosition;
            anchoredPos.y = newY;
            target.anchoredPosition = anchoredPos;
        }

        Debug.Log("🎯 skillButtonImages にサイズと位置を反映しました");
    }
}
