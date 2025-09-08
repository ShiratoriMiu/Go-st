using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public static LoginController Instance { get; private set; }

    public FirebaseUser User { get; private set; }
    public DatabaseReference DbReference { get; private set; }
    public bool IsFirebaseReady { get; private set; } = false;

    private TaskCompletionSource<bool> firebaseReadyTcs = new TaskCompletionSource<bool>();
    public Task WaitForFirebaseReadyAsync() => firebaseReadyTcs.Task;

    [SerializeField] InputField nameInputField;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        nameInputField.onEndEdit.AddListener(OnNameEndEdit);
    }

    private async void Start()
    {
        await InitializeFirebaseAsync();
    }

    /// <summary>
    /// Firebaseの初期化・匿名ログイン・ユーザー名設定まで行い準備完了を通知
    /// </summary>
    private async Task InitializeFirebaseAsync()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase初期化失敗: {dependencyStatus}");
                firebaseReadyTcs.TrySetException(new Exception($"Firebase initialization failed: {dependencyStatus}"));
                return;
            }

            var auth = FirebaseAuth.DefaultInstance;
            var loginResult = await auth.SignInAnonymouslyAsync();

            if (loginResult == null || auth.CurrentUser == null)
            {
                Debug.LogError("匿名ログイン失敗（ユーザー情報がnull）");
                firebaseReadyTcs.TrySetException(new Exception("Anonymous login failed"));
                return;
            }

            User = auth.CurrentUser;
            DbReference = FirebaseDatabase.DefaultInstance.RootReference;
            IsFirebaseReady = true;

            Debug.Log($"匿名ログイン成功: {User.UserId}");

            // 名前が未設定の場合のみデフォルト名を設定する（必要に応じて任意名に置き換え可能）
            if (string.IsNullOrEmpty(User.DisplayName))
            {
                await SetUserName("Player");
            }

            firebaseReadyTcs.TrySetResult(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase初期化中にエラー発生: {e}");
            firebaseReadyTcs.TrySetException(e);
        }
    }

    private void OnNameEndEdit(string newName)
    {
        // 非同期処理を発火（戻り値は使わない）
        _ = SetUserName(newName);
    }

    /// <summary>
    /// Firebaseユーザーの表示名を設定する
    /// </summary>
    public async Task SetUserName(string newName)
    {
        if (User == null || DbReference == null)
        {
            Debug.LogWarning("ユーザー情報がnullのため名前設定不可");
            return;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            newName = "Player";
        }

        try
        {
            // Firebase Auth の DisplayName を更新
            var profile = new UserProfile { DisplayName = newName };
            await User.UpdateUserProfileAsync(profile);

            // users/{uid}/displayName を更新
            await DbReference
                .Child("users")
                .Child(User.UserId)
                .Child("displayName")
                .SetValueAsync(newName);

            // rankings/{uid}/displayName も更新
            await DbReference
                .Child("rankings")
                .Child(User.UserId)
                .Child("displayName")
                .SetValueAsync(newName);

            Debug.Log($"ユーザー名設定成功: {newName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ユーザー名設定失敗: {e}");
        }
    }


    /// <summary>
    /// 現在のユーザー名を取得（未ログイン時は "NoName" を返す）
    /// </summary>
    public string GetUserName()
    {
        if (User == null)
        {
            Debug.LogWarning("ユーザーがログインしていません");
            return "NoName";
        }

        return User.DisplayName ?? "NoName";
    }
}
