using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private int maxLength = 10;        // 最大文字数
    [SerializeField] private string[] ngWords;          // NGワードリスト

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();
    }

    private void Start()
    {
        // 最大文字数設定
        inputField.characterLimit = maxLength;

        // 改行禁止
        inputField.lineType = InputField.LineType.SingleLine;

        // 入力チェック登録
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
    /// 入力中の文字列をチェック（改行削除＋最大文字数超過カット）
    /// </summary>
    private void OnValueChanged(string text)
    {
        string newText = text.Replace("\n", "").Replace("\r", ""); // 改行削除

        // 最大文字数超過分を切り捨て
        if (newText.Length > maxLength)
            newText = newText.Substring(0, maxLength);

        // 文字列が変化した場合だけ反映
        if (newText != text)
        {
            int caretPos = inputField.caretPosition;
            inputField.text = newText;
            inputField.caretPosition = Mathf.Min(caretPos, newText.Length);
        }
    }

    /// <summary>
    /// 入力確定時のチェック（NGワード判定）
    /// </summary>
    private void OnEndEdit(string text)
    {
        foreach (string ng in ngWords)
        {
            if (!string.IsNullOrEmpty(ng) && text.Contains(ng))
            {
                Debug.LogWarning($"NGワード「{ng}」が含まれています");
                inputField.text = ""; // 入力をクリア
                return;
            }
        }
    }
}
