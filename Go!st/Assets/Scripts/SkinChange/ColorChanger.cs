using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ColorChanger : MonoBehaviour
{
    private Renderer targetRenderer;

    [SerializeField] private Texture defaultTexture;
    [SerializeField] private SkinSlot[] skinSlots;
    [SerializeField] private Transform _colorScrollViewBase;
    [SerializeField] private GameObject scrollViewIconPrefab;

    [System.Serializable]
    public class SkinSlot
    {
        public string name;
        public Texture texture; // nullなら defaultTexture を使用
        public Color color;
        public Sprite icon;
        public bool isOwned;
    }

    private void Start()
    {
        // 生成前に既存の子オブジェクトを削除
        foreach (Transform child in _colorScrollViewBase.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (var skinSlot in skinSlots)
        {
            if (!skinSlot.isOwned) continue;

            // prefab を生成して ScrollView に配置
            GameObject slotObj = Instantiate(scrollViewIconPrefab, _colorScrollViewBase);

            // Button を取得
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
            {
                var capturedSkinSlot = skinSlot; // キャプチャ用
                button.onClick.AddListener(() =>
                {
                    ApplySkin(capturedSkinSlot);
                });
            }

            // 自分自身の Image を色変更
            Image buttonImage = slotObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = skinSlot.color;
            }

            Image childImage = slotObj.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != slotObj);
            // 子の Image にアイコン設定（icon がある場合）
            if (skinSlot.icon != null)
            {
                childImage.sprite = skinSlot.icon;
            }
            else
            {
                childImage.sprite = null;
                childImage.color = new Color(1, 1, 1, 0); // 完全透明
            }
        }
    }

    private void ApplySkin(SkinSlot skinSlot)
    {
        if (targetRenderer == null) return;

        Material[] materials = targetRenderer.materials;
        if (materials.Length == 0) return;

        // 0番目だけを複製して差し替え
        Material baseMat = new Material(materials[(int)MaterialSlot.Color]);
        baseMat.color = skinSlot.color;
        baseMat.mainTexture = skinSlot.texture != null ? skinSlot.texture : defaultTexture;

        materials[(int)MaterialSlot.Color] = baseMat; // 0番目だけ差し替え
        targetRenderer.materials = materials;
    }

    public void SetTargetRenderer(Renderer _targetRenderer)
    {
        targetRenderer = _targetRenderer;
    }
}
