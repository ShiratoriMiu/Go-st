using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Range(0, 1)]
    [SerializeField] private float _value = 1f;

    public float value
    {
        get => _value;
        set => SetValue(value);
    }

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    [Header("UI要素")]
    [SerializeField] private RectTransform background; // スライダー全体
    [SerializeField] private Image fillImage;          // Fill（fillAmount で変化）
    [SerializeField] private RectTransform handle;     // つまみ

    private bool suppressCallback = false;
    private float fullWidth;

    private void Start()
    {
        fullWidth = background.rect.width;
        UpdateUI();
    }

    public void SetValue(float v)
    {
        v = Mathf.Clamp01(v);

        if (Mathf.Approximately(_value, v))
            return;

        _value = v;
        UpdateUI();

        if (!suppressCallback)
            onValueChanged.Invoke(_value);
    }

    public void SetValueWithoutNotify(float v)
    {
        suppressCallback = true;
        SetValue(v);
        suppressCallback = false;
    }

    private void UpdateUI()
    {
        // Fill の描画量（絶対潰れない）
        fillImage.fillAmount = _value;

        // Handle の位置（fillAmount に合わせて右端へ）
        handle.anchoredPosition = new Vector2(
            -fullWidth / 2f + fullWidth * _value,
            handle.anchoredPosition.y
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateValueByPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateValueByPointer(eventData);
    }

    private void UpdateValueByPointer(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        float ratio = Mathf.InverseLerp(
            -background.rect.width / 2f,
            background.rect.width / 2f,
            localPos.x
        );

        SetValue(ratio);
    }
}
