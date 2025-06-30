using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    [Header("Firebase Controller")]
    [SerializeField] private FirebaseController firebaseController;

    [Header("�����L���O�\���ݒ�")]
    [SerializeField] private GameObject rankingTextBase;
    [SerializeField] private GameObject rankingTextPrefab;
    [SerializeField] private int rankingDisplayNum = 100;

    private readonly List<Text> rankingTextList = new List<Text>();
    private Task initializationTask;
    private Task firebaseReadyTask;

    private async void Start()
    {
        GenerateRankingTextUI();

        Debug.Log("[RankingManager] Firebase���������ҋ@��...");
        firebaseReadyTask = LoginController.Instance.WaitForFirebaseReadyAsync();
        await firebaseReadyTask;
        Debug.Log("[RankingManager] Firebase���������B�����L���O�������J�n...");

        initializationTask = InitializeRankingEntryAsync();
        await initializationTask;
        Debug.Log("[RankingManager] �����L���O�����������B");
    }


    /// <summary>
    /// �w�萔���̃����L���O�\���pText�I�u�W�F�N�g�𐶐�
    /// </summary>
    private void GenerateRankingTextUI()
    {
        for (int i = 0; i < rankingDisplayNum; ++i)
        {
            var text = Instantiate(rankingTextPrefab, rankingTextBase.transform)
                .GetComponentInChildren<Text>();
            rankingTextList.Add(text);
        }
    }

    /// <summary>
    /// �N���������L���O�G���g��������
    /// </summary>
    private async Task InitializeRankingEntryAsync()
    {
        if (firebaseController == null)
        {
            Debug.LogError("[RankingManager] FirebaseController���Z�b�g����Ă��܂���");
            return;
        }

        await firebaseController.EnsureRankingEntryAsync();
    }

    /// <summary>
    /// �����L���O�{�^���������ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    public async void OnRankingButtonClicked()
    {
        Debug.Log("[RankingManager] �����L���O�{�^�������B�����������ҋ@�J�n...");

        if (firebaseReadyTask != null)
        {
            await firebaseReadyTask;
            firebaseReadyTask = null;
        }

        if (initializationTask != null)
        {
            await initializationTask;
            initializationTask = null;
        }

        Debug.Log("[RankingManager] �����������B�����L���O�擾�J�n...");

        var topRanks = await firebaseController.GetTopRankingsAsync();
        Debug.Log($"�擾���������L���O��: {topRanks.Count}");
        DisplayRanking(topRanks);

        Debug.Log("[RankingManager] �����L���O�\�������B");
    }


    /// <summary>
    /// �����L���O����UI�ɕ\��
    /// </summary>
    private void DisplayRanking(List<(int rank, string name, int score)> rankings)
    {
        int displayCount = Mathf.Min(rankings.Count, rankingDisplayNum);

        for (int i = 0; i < displayCount; i++)
        {
            rankingTextList[i].text = $"{rankings[i].rank}��: {rankings[i].name}: {rankings[i].score}";
        }

        for (int i = displayCount; i < rankingDisplayNum; i++)
        {
            rankingTextList[i].text = string.Empty;
        }
    }
}
