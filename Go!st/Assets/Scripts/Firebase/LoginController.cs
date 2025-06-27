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

    // ? async�Ή��� Start()
    async void Start()
    {
        try
        {
            var result = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (result != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase ���������s: {result}");
                return;
            }

            var auth = FirebaseAuth.DefaultInstance;
            var loginResult = await auth.SignInAnonymouslyAsync();

            if (loginResult == null || auth.CurrentUser == null)
            {
                Debug.LogError("�������O�C�����s�i���[�U�[���null�j");
                return;
            }

            User = auth.CurrentUser;
            DbReference = FirebaseDatabase.DefaultInstance.RootReference;
            IsFirebaseReady = true;

            Debug.Log("�������O�C������: " + User.UserId);

            await SetUserName("Player"); // ? �񓯊��Ŗ��O�ݒ�
        }
        catch (Exception e)
        {
            Debug.LogError("Start���ɃG���[����: " + e.Message);
        }

        await EnsureRankingEntry();
    }

    // ? async�Ή��� SetUserName
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

            Debug.Log($"���[�U�[���ݒ萬��: {newName}");
        }
        catch (Exception e)
        {
            Debug.LogError("���[�U�[���ݒ莸�s: " + e.Message);
        }
    }

    public string GetUserName()
    {
        if (User == null)
        {
            Debug.LogWarning("���[�U�[�����O�C�����Ă��܂���");
            return "NoName";
        }

        string name = User.DisplayName ?? "NoName";
        Debug.Log($"���݂̃��[�U�[��: {name}");
        return name;
    }

    private async Task EnsureRankingEntry()
    {
        if (User == null || DbReference == null) return;

        var snapshot = await DbReference.Child("rankings").Child(User.UserId).GetValueAsync();
        if (!snapshot.Exists)
        {
            string name = User.DisplayName ?? "NoName";
            var data = new RankingData(name, 0);
            string json = JsonUtility.ToJson(data);
            await DbReference.Child("rankings").Child(User.UserId).SetRawJsonValueAsync(json);
            Debug.Log("�����L���O�G���g�������������܂����B");
        }
    }
}
