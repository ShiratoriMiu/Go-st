using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;    // ← これが必要（Selectable取得のため）
using DG.Tweening;

public class UIButtonPressFX : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Range(0.5f, 1f)]
    private float pressScaleMultiplier = 0.9f;

    private bool isPressed = false;
    private Vector3 originalScale;
    private Vector3 pressedScale;

    private Selectable selectable;   // ← Button / Toggle などまとめて扱える

    private void Awake()
    {
        originalScale = transform.localScale;
        pressedScale = originalScale * pressScaleMultiplier;

        selectable = GetComponent<Selectable>(); // Button/TMP_Button/Toggle全部OK
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // interactable = false なら押した時の演出を無効化
        if (selectable != null && !selectable.interactable)
            return;

        isPressed = true;
        Press();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        Release();
    }

    private void Press()
    {
        transform.DOKill();
        transform.DOScale(pressedScale, 0.05f).SetEase(Ease.OutQuad);
    }

    private void Release()
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.05f).SetEase(Ease.OutQuad);
    }
}
