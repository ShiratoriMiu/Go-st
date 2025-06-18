using UnityEngine;
using UnityEngine.UI;

public class TogglePressedLookButton : MonoBehaviour
{
    Button button;

    private Color normalColor;
    private Color pressedColor;
    private bool isPressedLook = false;

    void Start()
    {
        button = GetComponent<Button>();

        // ボタンの色設定を取得
        ColorBlock colors = button.colors;
        normalColor = colors.normalColor;
        pressedColor = colors.pressedColor;

        // 初期状態でボタンのTransitionを止める
        button.transition = Selectable.Transition.None;
    }

    // ボタンを押したときに呼ばれる処理
    public void SetPressedLook()
    {
        isPressedLook = true;
        button.image.color = pressedColor;
    }

    // 別のタイミングで戻す（例：3秒後に戻す）
    public void ResetButtonLook()
    {
        isPressedLook = false;
        button.image.color = normalColor;
    }
}
