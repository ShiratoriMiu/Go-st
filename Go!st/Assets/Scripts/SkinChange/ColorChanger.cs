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

        Material matInstance = new Material(targetRenderer.material); // マテリアルの複製
        Image image = skinButton.button.GetComponent<Image>();// Imageの使用

        if (image != null)
        {
            matInstance.color = image.color;
        }

        if (skinButton.texture != null)
        {
            matInstance.mainTexture = skinButton.texture;
        }
        else
        {
            // デフォルトをセット
            matInstance.mainTexture = defaultTexture;
        }

        targetRenderer.material = matInstance;
    }


    public void SetTargetRenderer(Renderer _targetRenderer)
    {
        targetRenderer = _targetRenderer;
    }
}
