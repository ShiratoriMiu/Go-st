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
        public Texture texture; // null�Ȃ� defaultTexture ���g�p
        public Color color;
        public Sprite icon;
        public bool isOwned;
    }

    private void Start()
    {
        // �����O�Ɋ����̎q�I�u�W�F�N�g���폜
        foreach (Transform child in _colorScrollViewBase.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (var skinSlot in skinSlots)
        {
            if (!skinSlot.isOwned) continue;

            // prefab �𐶐����� ScrollView �ɔz�u
            GameObject slotObj = Instantiate(scrollViewIconPrefab, _colorScrollViewBase);

            // Button ���擾
            Button button = slotObj.GetComponent<Button>();
            if (button != null)
            {
                var capturedSkinSlot = skinSlot; // �L���v�`���p
                button.onClick.AddListener(() =>
                {
                    ApplySkin(capturedSkinSlot);
                });
            }

            // �������g�� Image ��F�ύX
            Image buttonImage = slotObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = skinSlot.color;
            }

            Image childImage = slotObj.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != slotObj);
            // �q�� Image �ɃA�C�R���ݒ�iicon ������ꍇ�j
            if (skinSlot.icon != null)
            {
                childImage.sprite = skinSlot.icon;
            }
            else
            {
                childImage.sprite = null;
                childImage.color = new Color(1, 1, 1, 0); // ���S����
            }
        }
    }

    private void ApplySkin(SkinSlot skinSlot)
    {
        if (targetRenderer == null) return;

        Material[] materials = targetRenderer.materials;
        if (materials.Length == 0) return;

        // 0�Ԗڂ����𕡐����č����ւ�
        Material baseMat = new Material(materials[(int)MaterialSlot.Color]);
        baseMat.color = skinSlot.color;
        baseMat.mainTexture = skinSlot.texture != null ? skinSlot.texture : defaultTexture;

        materials[(int)MaterialSlot.Color] = baseMat; // 0�Ԗڂ��������ւ�
        targetRenderer.materials = materials;
    }

    public void SetTargetRenderer(Renderer _targetRenderer)
    {
        targetRenderer = _targetRenderer;
    }
}
