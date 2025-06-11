using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeUpManager : MonoBehaviour
{
    private Renderer targetRenderer;

    [System.Serializable]
    public class MakeUpButton
    {
        public Button makeUpButton;
        public Material makeUpMaterial;
        public MaterialSlot slotType;
        [HideInInspector] public bool isEquipped;
    }

    [SerializeField] private MakeUpButton[] makeUpButtons;

    // 透明マテリアルをキャッシュして返す
    private Material _transparentMaterial;

    void Start()
    {
        foreach (var button in makeUpButtons)
        {
            if (button.slotType == MaterialSlot.Color)
            {
                Debug.LogWarning($"[MakeUpManager] Button '{button.makeUpButton.name}' has slotType 'Color' which is not allowed for makeup. Button will be disabled in Inspector.");
                button.makeUpButton.interactable = false;
                continue;
            }

            var captured = button;
            button.makeUpButton.onClick.AddListener(() =>
            {
                ToggleMakeup(captured);
            });
        }
    }


    void ToggleMakeup(MakeUpButton button)
    {
        if (button.isEquipped)
        {
            RemoveMaterialAt(button);
        }
        else
        {
            ApplyMaterial(button);
        }
    }

    void ApplyMaterial(MakeUpButton button)
    {
        int slotIndex = (int)button.slotType;
        EnsureMaterialLength(slotIndex + 1);

        var mats = targetRenderer.materials;
        mats[slotIndex] = button.makeUpMaterial;
        targetRenderer.materials = mats;

        button.isEquipped = true;
    }

    void RemoveMaterialAt(MakeUpButton button)
    {
        int slotIndex = (int)button.slotType;
        var mats = targetRenderer.materials;
        if (slotIndex >= mats.Length) return;

        mats[slotIndex] = GetTransparentMaterial();
        targetRenderer.materials = mats;

        button.isEquipped = false;
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
}
