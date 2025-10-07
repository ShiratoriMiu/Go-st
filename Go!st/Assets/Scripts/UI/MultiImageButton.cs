using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class MultiGraphicButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button targetButton; // 操作対象のボタン

    [Header("対象となる Image / Text / TMP_Text")]
    [SerializeField] private List<Graphic> targetGraphics = new List<Graphic>(); // すべてGraphicで統一

    private List<Color> originalColors = new List<Color>();
    private ColorBlock colorBlock;

    private bool isPressed = false;
    private bool isHighlighted = false;

    void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        colorBlock = targetButton.colors;

        // 各Graphicの元の色を記録
        originalColors.Clear();
        foreach (var g in targetGraphics)
            originalColors.Add(g ? g.color : Color.white);
    }

    void Update()
    {
        if (targetButton == null) return;

        // 状態に応じた色選択
        Color stateColor = colorBlock.normalColor;

        if (!targetButton.interactable)
        {
            stateColor = colorBlock.disabledColor;
        }
        else if (isPressed)
        {
            stateColor = colorBlock.pressedColor;
        }
        else if (isHighlighted)
        {
            stateColor = colorBlock.highlightedColor;
        }

        // 各Graphicに乗算カラー適用
        for (int i = 0; i < targetGraphics.Count; i++)
        {
            var g = targetGraphics[i];
            if (g == null) continue;

            Color finalColor = MultiplyColor(originalColors[i], stateColor);
            g.CrossFadeColor(finalColor, colorBlock.fadeDuration, true, true);
        }
    }

    private Color MultiplyColor(Color baseColor, Color tintColor)
    {
        return new Color(
            baseColor.r * tintColor.r,
            baseColor.g * tintColor.g,
            baseColor.b * tintColor.b,
            baseColor.a * tintColor.a
        );
    }

    // ---- Pointer Events ----
    public void OnPointerDown(PointerEventData eventData) => isPressed = true;
    public void OnPointerUp(PointerEventData eventData) => isPressed = false;
    public void OnPointerEnter(PointerEventData eventData) => isHighlighted = true;
    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
    }
}
