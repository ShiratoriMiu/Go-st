using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class MultiGraphicButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button targetButton; // ����Ώۂ̃{�^��

    [Header("�ΏۂƂȂ� Image / Text / TMP_Text")]
    [SerializeField] private List<Graphic> targetGraphics = new List<Graphic>(); // ���ׂ�Graphic�œ���

    private List<Color> originalColors = new List<Color>();
    private ColorBlock colorBlock;

    private bool isPressed = false;
    private bool isHighlighted = false;

    void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        colorBlock = targetButton.colors;

        // �eGraphic�̌��̐F���L�^
        originalColors.Clear();
        foreach (var g in targetGraphics)
            originalColors.Add(g ? g.color : Color.white);
    }

    void Update()
    {
        if (targetButton == null) return;

        // ��Ԃɉ������F�I��
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

        // �eGraphic�ɏ�Z�J���[�K�p
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
