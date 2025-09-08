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
    /// Firebase�̏������E�������O�C���E���[�U�[���ݒ�܂ōs������������ʒm
    /// </summary>
    private async Task InitializeFirebaseAsync()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase���������s: {dependencyStatus}");
                firebaseReadyTcs.TrySetException(new Exception($"Firebase initialization failed: {dependencyStatus}"));
                return;
            }

            var auth = FirebaseAuth.DefaultInstance;
            var loginResult = await auth.SignInAnonymouslyAsync();

            if (loginResult == null || auth.CurrentUser == null)
            {
                Debug.LogError("�������O�C�����s�i���[�U�[���null�j");
                firebaseReadyTcs.TrySetException(new Exception("Anonymous login failed"));
                return;
            }

            User = auth.CurrentUser;
            DbReference = FirebaseDatabase.DefaultInstance.RootReference;
            IsFirebaseReady = true;

            Debug.Log($"�������O�C������: {User.UserId}");

            // ���O�����ݒ�̏ꍇ�̂݃f�t�H���g����ݒ肷��i�K�v�ɉ����ĔC�Ӗ��ɒu�������\�j
            if (string.IsNullOrEmpty(User.DisplayName))
            {
                await SetUserName("Player");
            }

            firebaseReadyTcs.TrySetResult(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase���������ɃG���[����: {e}");
            firebaseReadyTcs.TrySetException(e);
        }
    }

    private void OnNameEndEdit(string newName)
    {
        // �񓯊������𔭉΁i�߂�l�͎g��Ȃ��j
        _ = SetUserName(newName);
    }

    /// <summary>
    /// Firebase���[�U�[�̕\������ݒ肷��
    /// </summary>
    public async Task SetUserName(string newName)
    {
        if (User == null || DbReference == null)
        {
            Debug.LogWarning("���[�U�[���null�̂��ߖ��O�ݒ�s��");
            return;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            newName = "Player";
        }

        try
        {
            // Firebase Auth �� DisplayName ���X�V
            var profile = new UserProfile { DisplayName = newName };
            await User.UpdateUserProfileAsync(profile);

            // users/{uid}/displayName ���X�V
            await DbReference
                .Child("users")
                .Child(User.UserId)
                .Child("displayName")
                .SetValueAsync(newName);

            // rankings/{uid}/displayName ���X�V
            await DbReference
                .Child("rankings")
                .Child(User.UserId)
                .Child("displayName")
                .SetValueAsync(newName);

            Debug.Log($"���[�U�[���ݒ萬��: {newName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"���[�U�[���ݒ莸�s: {e}");
        }
    }


    /// <summary>
    /// ���݂̃��[�U�[�����擾�i�����O�C������ "NoName" ��Ԃ��j
    /// </summary>
    public string GetUserName()
    {
        if (User == null)
        {
            Debug.LogWarning("���[�U�[�����O�C�����Ă��܂���");
            return "NoName";
        }

        return User.DisplayName ?? "NoName";
    }
}
