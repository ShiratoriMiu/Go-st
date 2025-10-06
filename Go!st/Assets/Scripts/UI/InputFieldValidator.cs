using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private Text textComponent; // InputFieldのText
    [SerializeField] private string csvFileName = "NGWords";

    [Header("文字サイズ自動調整")]
    [SerializeField] private int fontSizeMin = 10;
    [SerializeField] private int fontSizeMax = 40;

    private string[] ngWords;
    private const int maxJapaneseLength = 6;
    private const int maxEnglishLength = 12;

    public event Action<string> OnValidatedName; // 確定後の文字列を通知

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
            Debug.LogWarning($"CSVファイル '{csvFileName}.csv' が見つかりません。");
            ngWords = new string[0];
            return;
        }

        ngWords = csvData.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    // 入力中は改行削除のみ
    private void OnValueChanged(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // 改行削除
        string newText = text.Replace("\n", "").Replace("\r", "");

        // 重み付き長さ制限
        int maxWeightedLength = maxJapaneseLength * 2; // 6全角 or 12半角相当
        if (GetWeightedLength(newText) > maxWeightedLength)
        {
            newText = CutToWeightedLimit(newText, maxWeightedLength);
        }

        // 文字列が変化したら反映
        if (newText != text)
        {
            int caretPos = inputField.caretPosition;
            inputField.text = newText;
            inputField.caretPosition = Mathf.Min(caretPos, newText.Length);
        }
    }


    // 確定時にNGワードチェック・文字数制限・文字サイズ調整
    private void OnEndEdit(string text)
    {
        string validatedText = text;

        // NGワードチェック
        foreach (string ng in ngWords)
        {
            if (!string.IsNullOrEmpty(ng) && validatedText.Contains(ng))
            {
                Debug.LogWarning($"NGワード「{ng}」が含まれています");
                validatedText = "";
                break;
            }
        }

        // 文字数制限
        int maxWeightedLength = maxJapaneseLength * 2;
        if (GetWeightedLength(validatedText) > maxWeightedLength)
        {
            validatedText = CutToWeightedLimit(validatedText, maxWeightedLength);
        }

        // InputField に反映
        inputField.text = validatedText;

        // ここで Firebase に通知
        OnValidatedName?.Invoke(validatedText);

        // 文字サイズ調整などもここでOK
        AdjustFontSize(validatedText);
    }

    // BestFit風の文字サイズ調整
    private void AdjustFontSize(string text)
    {
        if (textComponent == null || string.IsNullOrEmpty(text))
            return;

        int size = fontSizeMax;
        textComponent.fontSize = size;

        // Canvas更新して正しい幅を取得
        Canvas.ForceUpdateCanvases();

        RectTransform rt = textComponent.rectTransform;
        float maxWidth = rt.rect.width;

        // TextGeneratorを使って文字幅を正確に計算
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
    // 文字数制限（重み付き）
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
