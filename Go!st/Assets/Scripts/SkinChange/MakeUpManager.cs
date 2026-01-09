using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ColorChanger;

public class MakeUpManager : MonoBehaviour
{
    private Renderer targetRenderer;

    [System.Serializable]
    public class MakeUpSlot
    {
        public string name;
        public Material makeUpMaterial;
        public MaterialSlot slotType;
        public Sprite icon;
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
        PlayerSaveData playerData = SaveManager.Load();
        RestoreMakeEquipped(playerData);
    }

    void OnEnable()
    {
        RefreshMakeUpUI();
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

        // ★ 同じ slotType の他メイクを解除
        foreach (var s in makeUpSlots)
        {
            if (s == slot) continue;
            if (s.slotType != slot.slotType) continue;
            if (!s.isEquipped) continue;

            s.isEquipped = false;
        }

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

    public void RefreshMakeUpUI()
    {
        // データ再ロード
        PlayerSaveData playerData = SaveManager.Load();
        RestoreMakeEquipped(playerData);

        // 既存UI削除
        foreach (Transform child in _makeUpScrollViewBase)
        {
            Destroy(child.gameObject);
        }

        // 再生成
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

            Image childImage = btn.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img => img.transform.parent == btn.transform);

            Text text = go.GetComponentsInChildren<Text>()
                          .FirstOrDefault(t => t.gameObject != go);

            // 子の Image にアイコン設定（icon がある場合）
            if (slot.icon != null)
            {
                childImage.sprite = slot.icon;
                childImage.color = new Color(1, 1, 1, 1); // 完全透明
                if (text != null)
                    text.text = "";
            }
            else
            {
                childImage.sprite = null;
                childImage.color = new Color(1, 1, 1, 0); // 完全透明
                if (text != null)
                    text.text = slot.name;
            }
        }
    }
}
