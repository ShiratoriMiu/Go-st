using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Threading.Tasks;
using System.Net.NetworkInformation; // �� �C���^�[�l�b�g�m�F�p

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

        TryLoadRanking(); // �N�����ɓǂݍ��݂�����
    }

    void TryLoadRanking()
    {
        errorPanel.SetActive(false);
        retryButton.interactable = false; // ?? �ʐM���͉����Ȃ�

        if (!IsConnectedToInternet())
        {
            ShowError("�C���^�[�l�b�g�ɐڑ�����Ă��܂���B");
            retryButton.interactable = true; // ?? �I�t���C���Ȃ炷���߂�
            return;
        }

        // ���o�C���ʐM���Ȃ�x���\���i�ڑ��͎��s����j
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            ShowWarning("���݃��o�C���f�[�^�ʐM���ł��B\n�ʐM����������Ă��Ȃ����A�ݒ�����m�F���������B");
        }

        dbRef.GetValueAsync().ContinueWith(OnDataReceived);
    }

    void OnDataReceived(Task<DataSnapshot> task)
    {
        // �����E���s�Ɋւ�炸�A�ʐM�I�����ɍēx�{�^����������悤�ɂ���
        retryButton.interactable = true; // ??

        if (task.IsFaulted)
        {
            string reason = GetErrorReason(task.Exception);
            ShowError("�ʐM�Ɏ��s���܂����F" + reason);
        }
        else if (task.IsCanceled)
        {
            ShowError("�ʐM���L�����Z������܂����B");
        }
        else
        {
            errorPanel.SetActive(false);
            // �ʐM�������̏����i��F�����L���O�\���j
            Debug.Log("�����L���O�擾�����I");
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

    // �C���^�[�l�b�g�ڑ��m�F
    bool IsConnectedToInternet()
    {
#if UNITY_ANDROID || UNITY_IOS
        return Application.internetReachability != NetworkReachability.NotReachable;
#else
        return NetworkInterface.GetIsNetworkAvailable(); // PC�Ȃǂł̊m�F
#endif
    }

    // ��O���猴���𒊏o
    string GetErrorReason(System.AggregateException exception)
    {
        if (exception == null) return "�s���ȃG���[";

        if (exception.InnerExceptions.Count > 0)
        {
            string msg = exception.InnerExceptions[0].Message;
            if (msg.Contains("timed out")) return "�^�C���A�E�g";
            if (msg.Contains("unable to resolve host")) return "DNS�G���[";
            if (msg.Contains("permission")) return "Firebase���[���ɂ�苑�ۂ���܂���";
            // ���ɂ��ǉ��\
            return msg;
        }

        return "�s���ȃG���[";
    }
}
