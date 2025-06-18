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

        // �{�^���̐F�ݒ���擾
        ColorBlock colors = button.colors;
        normalColor = colors.normalColor;
        pressedColor = colors.pressedColor;

        // ������ԂŃ{�^����Transition���~�߂�
        button.transition = Selectable.Transition.None;
    }

    // �{�^�����������Ƃ��ɌĂ΂�鏈��
    public void SetPressedLook()
    {
        isPressedLook = true;
        button.image.color = pressedColor;
    }

    // �ʂ̃^�C�~���O�Ŗ߂��i��F3�b��ɖ߂��j
    public void ResetButtonLook()
    {
        isPressedLook = false;
        button.image.color = normalColor;
    }
}
