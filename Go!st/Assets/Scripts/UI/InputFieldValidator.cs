using UnityEngine;
using UnityEngine.UI;

public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private int maxLength = 10;   // �ő啶����
    [SerializeField] private string[] ngWords;     // NG���[�h���X�g

    private void Start()
    {
        // �ő啶�����ݒ�
        inputField.characterLimit = maxLength;

        // ���s�֎~
        inputField.lineType = InputField.LineType.SingleLine;

        // ���̓`�F�b�N�o�^
        inputField.onValueChanged.AddListener(OnValueChanged);
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnValueChanged(string text)
    {
        // ���s���������ꍇ�͍폜
        if (text.Contains("\n") || text.Contains("\r"))
        {
            inputField.text = text.Replace("\n", "").Replace("\r", "");
        }
    }

    private void OnEndEdit(string text)
    {
        foreach (string ng in ngWords)
        {
            if (!string.IsNullOrEmpty(ng) && text.Contains(ng))
            {
                Debug.LogWarning($"NG���[�h�u{ng}�v���܂܂�Ă��܂�");
                inputField.text = ""; // ���͂��N���A
                break;
            }
        }
    }
}
