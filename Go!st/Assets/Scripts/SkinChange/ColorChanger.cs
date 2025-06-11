using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    private Renderer targetRenderer;

    [SerializeField] Texture defaultTexture;
    [SerializeField] private SkinButton[] skinButtons;

    [System.Serializable]
    public class SkinButton
    {
        public Button button;
        public Texture texture; // nullなら Image の color を使用
    }

    private void Start()
    {
        foreach (var skinButton in skinButtons)
        {
            if (skinButton.button != null)
            {
                var capturedSkinButton = skinButton; // ラムダ内でキャプチャするため

                skinButton.button.onClick.AddListener(() =>
                {
                    ApplySkin(capturedSkinButton);
                });
            }
        }
    }

    private void ApplySkin(SkinButton skinButton)
    {
        if (targetRenderer == null) return;

        Material[] materials = targetRenderer.materials;

        if (materials.Length == 0)
        {
            return;
        }

        // 0番目だけを複製して差し替える
        Material baseMat = new Material(materials[(int)MaterialSlot.Color]);

        Image image = skinButton.button.GetComponent<Image>();
        if (image != null)
        {
            baseMat.color = image.color;
        }

        baseMat.mainTexture = skinButton.texture != null ? skinButton.texture : defaultTexture;

        materials[(int)MaterialSlot.Color] = baseMat; // 0番目だけ差し替え
        targetRenderer.materials = materials;
    }

    public void SetTargetRenderer(Renderer _targetRenderer)
    {
        targetRenderer = _targetRenderer;
    }
}
