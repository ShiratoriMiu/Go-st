using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class LoginController : MonoBehaviour
{
    public static LoginController Instance { get; private set; }

    public FirebaseUser User { get; private set; }
    public DatabaseReference DbReference { get; private set; }
    public bool IsFirebaseReady { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ? async対応版 Start()
    async void Start()
    {
        try
        {
            var result = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (result != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase 初期化失敗: {result}");
                return;
            }

            var auth = FirebaseAuth.DefaultInstance;
            var loginResult = await auth.SignInAnonymouslyAsync();

            if (loginResult == null || auth.CurrentUser == null)
            {
                Debug.LogError("匿名ログイン失敗（ユーザー情報がnull）");
                return;
            }

            User = auth.CurrentUser;
            DbReference = FirebaseDatabase.DefaultInstance.RootReference;
            IsFirebaseReady = true;

            Debug.Log("匿名ログイン成功: " + User.UserId);

            await SetUserName("Player"); // ? 非同期で名前設定
        }
        catch (Exception e)
        {
            Debug.LogError("Start中にエラー発生: " + e.Message);
        }
    }

    // ? async対応版 SetUserName
    public async Task SetUserName(string newName)
    {
        if (User == null || DbReference == null) return;

        try
        {
            var profile = new UserProfile { DisplayName = newName };
            await User.UpdateUserProfileAsync(profile);

            await DbReference
                .Child("users")
                .Child(User.UserId)
                .Child("displayName")
                .SetValueAsync(newName);

            Debug.Log($"ユーザー名設定成功: {newName}");
        }
        catch (Exception e)
        {
            Debug.LogError("ユーザー名設定失敗: " + e.Message);
        }
    }
}
