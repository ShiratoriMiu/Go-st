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

    // �{�^�����������Ƃ��ɌĂ΂�鏈��
    public void SetPressedLook()
    {
        EnsureInit();
        isPressedLook = true;
        button.image.color = pressedColor;
    }

    // �ʂ̃^�C�~���O�Ŗ߂��i��F3�b��ɖ߂��j
    public void ResetButtonLook()
    {
        EnsureInit();
        isPressedLook = false;
        button.image.color = normalColor;
    }
}
