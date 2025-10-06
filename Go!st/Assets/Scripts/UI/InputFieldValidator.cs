using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private Text textComponent; // InputField��Text
    [SerializeField] private string csvFileName = "NGWords";

    [Header("�����T�C�Y��������")]
    [SerializeField] private int fontSizeMin = 10;
    [SerializeField] private int fontSizeMax = 40;

    private string[] ngWords;
    private const int maxJapaneseLength = 6;
    private const int maxEnglishLength = 12;

    public event Action<string> OnValidatedName; // �m���̕������ʒm

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (textComponent == null)
            textComponent = inputField.textComponent;

        LoadNGWordsFromCSV();
    }

    private void Start()
    {
        inputField.lineType = InputField.LineType.SingleLine;

        inputField.onValueChanged.AddListener(OnValueChanged);
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEnable()
    {
        string playerName = LoginController.Instance.GetUserName();
        if (!string.IsNullOrEmpty(playerName) && playerName != "Player")
            inputField.text = playerName;
    }

    private void LoadNGWordsFromCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);
        if (csvData == null)
        {
            Debug.LogWarning($"CSV�t�@�C�� '{csvFileName}.csv' ��������܂���B");
            ngWords = new string[0];
            return;
        }

        ngWords = csvData.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    // ���͒��͉��s�폜�̂�
    private void OnValueChanged(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // ���s�폜
        string newText = text.Replace("\n", "").Replace("\r", "");

        // �d�ݕt����������
        int maxWeightedLength = maxJapaneseLength * 2; // 6�S�p or 12���p����
        if (GetWeightedLength(newText) > maxWeightedLength)
        {
            newText = CutToWeightedLimit(newText, maxWeightedLength);
        }

        // �����񂪕ω������甽�f
        if (newText != text)
        {
            int caretPos = inputField.caretPosition;
            inputField.text = newText;
            inputField.caretPosition = Mathf.Min(caretPos, newText.Length);
        }
    }


    // �m�莞��NG���[�h�`�F�b�N�E�����������E�����T�C�Y����
    private void OnEndEdit(string text)
    {
        string validatedText = text;

        // NG���[�h�`�F�b�N
        foreach (string ng in ngWords)
        {
            if (!string.IsNullOrEmpty(ng) && validatedText.Contains(ng))
            {
                Debug.LogWarning($"NG���[�h�u{ng}�v���܂܂�Ă��܂�");
                validatedText = "";
                break;
            }
        }

        // ����������
        int maxWeightedLength = maxJapaneseLength * 2;
        if (GetWeightedLength(validatedText) > maxWeightedLength)
        {
            validatedText = CutToWeightedLimit(validatedText, maxWeightedLength);
        }

        // InputField �ɔ��f
        inputField.text = validatedText;

        // ������ Firebase �ɒʒm
        OnValidatedName?.Invoke(validatedText);

        // �����T�C�Y�����Ȃǂ�������OK
        AdjustFontSize(validatedText);
    }

    // BestFit���̕����T�C�Y����
    private void AdjustFontSize(string text)
    {
        if (textComponent == null || string.IsNullOrEmpty(text))
            return;

        int size = fontSizeMax;
        textComponent.fontSize = size;

        // Canvas�X�V���Đ����������擾
        Canvas.ForceUpdateCanvases();

        RectTransform rt = textComponent.rectTransform;
        float maxWidth = rt.rect.width;

        // TextGenerator���g���ĕ������𐳊m�Ɍv�Z
        TextGenerationSettings settings = textComponent.GetGenerationSettings(rt.rect.size);

        while (size > fontSizeMin)
        {
            settings.fontSize = size;
            float preferredWidth = textComponent.cachedTextGeneratorForLayout.GetPreferredWidth(text, settings) / textComponent.pixelsPerUnit;

            if (preferredWidth <= maxWidth)
                break;

            size--;
            textComponent.fontSize = size;
        }
    }

    // ======================
    // �����������i�d�ݕt���j
    // ======================
    private int GetWeightedLength(string text)
    {
        int length = 0;
        foreach (char c in text)
        {
            length += GetCharWeight(c);
        }
        return length;
    }

    private string CutToWeightedLimit(string text, int maxWeightedLength)
    {
        int length = 0;
        StringBuilder sb = new StringBuilder();
        foreach (char c in text)
        {
            int weight = GetCharWeight(c);
            if (length + weight > maxWeightedLength)
                break;
            sb.Append(c);
            length += weight;
        }
        return sb.ToString();
    }

    private int GetCharWeight(char c)
    {
        if (IsFullWidth(c)) return 2;
        if (IsUppercaseLetter(c)) return 2;
        return 1;
    }

    private bool IsFullWidth(char c) => c > 0xFF;
    private bool IsUppercaseLetter(char c) => c >= 'A' && c <= 'Z';
}
