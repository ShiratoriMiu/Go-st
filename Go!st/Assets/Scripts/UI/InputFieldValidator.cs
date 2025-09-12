using UnityEngine;
using UnityEngine.UI;

public class InputFieldValidator : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private int maxLength = 10;   // 最大文字数
    [SerializeField] private string[] ngWords;     // NGワードリスト

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

    private void OnValueChanged(string text)
    {
        // 改行が入った場合は削除
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
                Debug.LogWarning($"NGワード「{ng}」が含まれています");
                inputField.text = ""; // 入力をクリア
                break;
            }
        }
    }
}
