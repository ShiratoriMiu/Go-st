using UnityEngine;
using UnityEngine.UI;

public class TogglePressedLookButton : MonoBehaviour
{
    Button button;

    private Color normalColor;
    private Color pressedColor;
    private bool isPressedLook = false;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (button != null) return;

        button = GetComponent<Button>();
        if (button != null)
        {
            ColorBlock colors = button.colors;
            normalColor = colors.normalColor;
            pressedColor = colors.pressedColor;
            button.transition = Selectable.Transition.None;
        }
    }

    public void EnsureInit() => Init();

    // ボタンを押したときに呼ばれる処理
    public void SetPressedLook()
    {
        EnsureInit();
        isPressedLook = true;
        button.image.color = pressedColor;
    }

    // 別のタイミングで戻す（例：3秒後に戻す）
    public void ResetButtonLook()
    {
        EnsureInit();
        isPressedLook = false;
        button.image.color = normalColor;
    }
}
