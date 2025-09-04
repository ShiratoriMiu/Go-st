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

    // 透明マテリアルをキャッシュして返す
    private Material _transparentMaterial;

    void Start()
    {
        // JSONからロード
        PlayerSaveData playerData = SaveManager.Load();

        RestoreMakeEquipped(playerData);

        if (_makeUpScrollViewBase == null) return;
        
        // 既存の子オブジェクト削除
        foreach (Transform child in _makeUpScrollViewBase)
        {
            Destroy(child.gameObject);
        }

        // isOwned == true のスロットだけUI生成
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
            // 子の Image にアイコン設定（icon がある場合）
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
            extended.Add(new Material(Shader.Find("Standard"))); // 空白で拡張
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

        // まず一旦すべて解除
        foreach (var makeSlot in makeUpSlots)
        {
            makeSlot.isEquipped = false;
        }

        foreach (var makeSlot in makeUpSlots)
        {
            if (makeSlot == null) continue;

            // セーブデータから該当する ItemData を探す
            ItemData savedItem = allItems.Find(i => i.name == makeSlot.name);
            if (savedItem == null) continue;

            // 所持状態の復元
            makeSlot.isOwned = savedItem.isOwned;

            // 装備状態をを復元
            makeSlot.isEquipped = savedItem.isEquipped;
        }
    }
}
