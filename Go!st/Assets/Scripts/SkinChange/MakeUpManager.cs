using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MakeUpManager : MonoBehaviour
{
    private Renderer targetRenderer;

    [System.Serializable]
    public class MakeUpSlot
    {
        public string name;
        public Material makeUpMaterial;
        public MaterialSlot slotType;
        public bool isOwned;
        [HideInInspector] public bool isEquipped;
        public bool isGacha;
    }

    [SerializeField] private MakeUpSlot[] makeUpSlots;
    [SerializeField] private Transform _makeUpScrollViewBase;
    [SerializeField] private GameObject scrollViewIconPrefab;

    public IReadOnlyList<MakeUpSlot> MakeUpSlots => makeUpSlots;

    // �����}�e���A�����L���b�V�����ĕԂ�
    private Material _transparentMaterial;

    void Start()
    {
        // JSON���烍�[�h
        PlayerSaveData playerData = SaveManager.Load();

        RestoreMakeEquipped(playerData);

        if (_makeUpScrollViewBase == null) return;
        
        // �����̎q�I�u�W�F�N�g�폜
        foreach (Transform child in _makeUpScrollViewBase)
        {
            Destroy(child.gameObject);
        }

        // isOwned == true �̃X���b�g����UI����
        foreach (var slot in makeUpSlots)
        {
            if (!slot.isOwned) continue;

            var go = Instantiate(scrollViewIconPrefab, _makeUpScrollViewBase);
            Button btn = go.GetComponent<Button>();
            var captured = slot;

            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    ToggleMakeup(captured);
                });
            }

            Text childText = go.GetComponentsInChildren<Text>().FirstOrDefault(txt => txt.gameObject != go);
            // �q�� Image �ɃA�C�R���ݒ�iicon ������ꍇ�j
            if (go.name != null)
            {
                childText.text = slot.name;
            }
            else
            {
                childText.text = "";
            }
        }
    }

    void ToggleMakeup(MakeUpSlot slot)
    {
        if (slot.isEquipped)
        {
            RemoveMaterialAt(slot);
        }
        else
        {
            ApplyMaterial(slot);
        }
    }

    void ApplyMaterial(MakeUpSlot slot)
    {
        int slotIndex = (int)slot.slotType;
        EnsureMaterialLength(slotIndex + 1);

        var mats = targetRenderer.materials;
        mats[slotIndex] = slot.makeUpMaterial;
        targetRenderer.materials = mats;

        slot.isEquipped = true;
    }

    void RemoveMaterialAt(MakeUpSlot slot)
    {
        int slotIndex = (int)slot.slotType;
        var mats = targetRenderer.materials;
        if (slotIndex >= mats.Length) return;

        mats[slotIndex] = GetTransparentMaterial();
        targetRenderer.materials = mats;

        slot.isEquipped = false;
    }

    void EnsureMaterialLength(int requiredLength)
    {
        Material[] mats = targetRenderer.materials;
        if (mats.Length >= requiredLength) return;

        List<Material> extended = new List<Material>(mats);
        while (extended.Count < requiredLength)
        {
            extended.Add(new Material(Shader.Find("Standard"))); // �󔒂Ŋg��
        }

        targetRenderer.materials = extended.ToArray();
    }

    public void SetTargetRenderer(Renderer renderer)
    {
        targetRenderer = renderer;
    }

    Material GetTransparentMaterial()
    {
        if (_transparentMaterial == null)
        {
            _transparentMaterial = new Material(Shader.Find("Standard"));
            _transparentMaterial.SetFloat("_Mode", 3);
            _transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _transparentMaterial.SetInt("_ZWrite", 0);
            _transparentMaterial.DisableKeyword("_ALPHATEST_ON");
            _transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
            _transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _transparentMaterial.renderQueue = 3000;
            _transparentMaterial.color = new Color(1, 1, 1, 0);
        }
        return _transparentMaterial;
    }

    private void RestoreMakeEquipped(PlayerSaveData data)
    {
        List<ItemData> allItems = data.allItems ?? new List<ItemData>();

        // �܂���U���ׂĉ���
        foreach (var makeSlot in makeUpSlots)
        {
            makeSlot.isEquipped = false;
        }

        foreach (var makeSlot in makeUpSlots)
        {
            if (makeSlot == null) continue;

            // �Z�[�u�f�[�^����Y������ ItemData ��T��
            ItemData savedItem = allItems.Find(i => i.name == makeSlot.name);
            if (savedItem == null) continue;

            // ������Ԃ̕���
            makeSlot.isOwned = savedItem.isOwned;

            // ������Ԃ��𕜌�
            makeSlot.isEquipped = savedItem.isEquipped;
        }
    }
}
