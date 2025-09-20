using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private int maxLength = 10;        // �ő啶����
    [SerializeField] private string[] ngWords;          // NG���[�h���X�g

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();
    }

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

    private void OnEnable()
    {
        string playerName = LoginController.Instance.GetUserName();
        if(playerName != "Player")
        {
            inputField.text = playerName;
        }
    }

    /// <summary>
    /// ���͒��̕�������`�F�b�N�i���s�폜�{�ő啶�������߃J�b�g�j
    /// </summary>
    private void OnValueChanged(string text)
    {
        string newText = text.Replace("\n", "").Replace("\r", ""); // ���s�폜

        // �ő啶�������ߕ���؂�̂�
        if (newText.Length > maxLength)
            newText = newText.Substring(0, maxLength);

        // �����񂪕ω������ꍇ�������f
        if (newText != text)
        {
            int caretPos = inputField.caretPosition;
            inputField.text = newText;
            inputField.caretPosition = Mathf.Min(caretPos, newText.Length);
        }
    }

    /// <summary>
    /// ���͊m�莞�̃`�F�b�N�iNG���[�h����j
    /// </summary>
    private void OnEndEdit(string text)
    {
        foreach (string ng in ngWords)
        {
            if (!string.IsNullOrEmpty(ng) && text.Contains(ng))
            {
                Debug.LogWarning($"NG���[�h�u{ng}�v���܂܂�Ă��܂�");
                inputField.text = ""; // ���͂��N���A
                return;
            }
        }
    }
}
