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
}
