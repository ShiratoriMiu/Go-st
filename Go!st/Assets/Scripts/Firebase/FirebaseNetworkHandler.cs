using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Threading.Tasks;
using System.Net.NetworkInformation; // ← インターネット確認用

public class FirebaseNetworkHandler : MonoBehaviour
{
    public GameObject errorPanel;
    public TMP_Text errorText;
    public Button retryButton;

    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("ranking");

        errorPanel.SetActive(false);
        retryButton.onClick.AddListener(() => TryLoadRanking());

        TryLoadRanking(); // 起動時に読み込みを試す
    }

    void TryLoadRanking()
    {
        errorPanel.SetActive(false);
        retryButton.interactable = false; // ?? 通信中は押せない

        if (!IsConnectedToInternet())
        {
            ShowError("インターネットに接続されていません。");
            retryButton.interactable = true; // ?? オフラインならすぐ戻す
            return;
        }

        // モバイル通信中なら警告表示（接続は試行する）
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            ShowWarning("現在モバイルデータ通信中です。\n通信が制限されていないか、設定をご確認ください。");
        }

        dbRef.GetValueAsync().ContinueWith(OnDataReceived);
    }

    void OnDataReceived(Task<DataSnapshot> task)
    {
        // 成功・失敗に関わらず、通信終了時に再度ボタンを押せるようにする
        retryButton.interactable = true; // ??

        if (task.IsFaulted)
        {
            string reason = GetErrorReason(task.Exception);
            ShowError("通信に失敗しました：" + reason);
        }
        else if (task.IsCanceled)
        {
            ShowError("通信がキャンセルされました。");
        }
        else
        {
            errorPanel.SetActive(false);
            // 通信成功時の処理（例：ランキング表示）
            Debug.Log("ランキング取得成功！");
        }
    }

    void ShowError(string message)
    {
        errorText.text = "? " + message;
        errorPanel.SetActive(true);
    }

    void ShowWarning(string message)
    {
        errorText.text = "?? " + message;
        errorPanel.SetActive(true);
    }

    // インターネット接続確認
    bool IsConnectedToInternet()
    {
#if UNITY_ANDROID || UNITY_IOS
        return Application.internetReachability != NetworkReachability.NotReachable;
#else
        return NetworkInterface.GetIsNetworkAvailable(); // PCなどでの確認
#endif
    }

    // 例外から原因を抽出
    string GetErrorReason(System.AggregateException exception)
    {
        if (exception == null) return "不明なエラー";

        if (exception.InnerExceptions.Count > 0)
        {
            string msg = exception.InnerExceptions[0].Message;
            if (msg.Contains("timed out")) return "タイムアウト";
            if (msg.Contains("unable to resolve host")) return "DNSエラー";
            if (msg.Contains("permission")) return "Firebaseルールにより拒否されました";
            // 他にも追加可能
            return msg;
        }

        return "不明なエラー";
    }
}
