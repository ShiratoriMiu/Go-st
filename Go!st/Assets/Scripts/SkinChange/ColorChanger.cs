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
        public Texture texture; // null�Ȃ� Image �� color ���g�p
    }

    private void Start()
    {
        foreach (var skinButton in skinButtons)
        {
            if (skinButton.button != null)
            {
                var capturedSkinButton = skinButton; // �����_���ŃL���v�`�����邽��

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

        Material matInstance = new Material(targetRenderer.material); // �}�e���A���̕���
        Image image = skinButton.button.GetComponent<Image>();// Image�̎g�p

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
            // �f�t�H���g���Z�b�g
            matInstance.mainTexture = defaultTexture;
        }

        targetRenderer.material = matInstance;
    }


    public void SetTargetRenderer(Renderer _targetRenderer)
    {
        targetRenderer = _targetRenderer;
    }
}
