using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private FirebaseController firebaseController;

    [Header("�����L���OUI�ݒ�")]
    [SerializeField] private Transform rankingContainer; // VerticalLayoutGroup / GridLayoutGroup ����
    [SerializeField] private GameObject rankingTextBGPrefab;
    [SerializeField] private int maxDisplayCount = 100;

    private List<Text> rankingTextList = new List<Text>();
    private Task initializationTask;

    private bool _isFetchingTopRankings = false; // �A�Ŗh�~�p�t���O

    private async void Start()
    {
        Debug.Log("[RankingManager] Firebase���������ҋ@��...");
        await LoginController.Instance.WaitForFirebaseReadyAsync();
        Debug.Log("[RankingManager] Firebase���������B�����L���O�G���g���������J�n...");

        initializationTask = firebaseController.EnsureRankingEntryAsync();
        await initializationTask;

        Debug.Log("[RankingManager] �����L���O�G���g�������������B");

        GenerateRankingTextUI();
    }

    private void GenerateRankingTextUI()
    {
        // ���łɐ����ς݂�UI���폜
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }
        rankingTextList.Clear();

        for (int i = 0; i < maxDisplayCount; i++)
        {
            GameObject bg = Instantiate(rankingTextBGPrefab, rankingContainer);
            Text rankingText = bg.GetComponentInChildren<Text>();
            rankingText.text = $"No.{i + 1}  --- : ---";
            rankingTextList.Add(rankingText);
        }
    }

    public async void OnRankingButtonClicked()
    {
        if (_isFetchingTopRankings) return; // �A�Ŗh�~
        _isFetchingTopRankings = true;

        Debug.Log("[RankingManager] �����L���O�{�^�������B�����������ҋ@�J�n...");

        if (initializationTask != null)
        {
            await initializationTask;
            initializationTask = null;
        }

        Debug.Log("[RankingManager] �����������B�����L���O�擾�J�n...");

        const int MaxRetryCount = 10; // �ő�10��Ď擾�i��5�b�j
        int retryCount = 0;
        List<(int rank, string name, int score)> topRanks = null;

        while (retryCount < MaxRetryCount)
        {
            topRanks = await firebaseController.GetTopRankingsFromServerAsync(maxDisplayCount);

            if (topRanks.Count > 0)
            {
                Debug.Log($"[RankingManager] �����L���O�擾�����i���s��: {retryCount + 1}�j ����: {topRanks.Count}");
                break;
            }
            else
            {
                Debug.LogWarning($"[RankingManager] �����L���O�擾���ʂ�0���A�Ď擾���s�� ({retryCount + 1}/{MaxRetryCount})...");
                await Task.Delay(500); // 0.5�b�ҋ@
            }

            retryCount++;
        }

        if (topRanks == null || topRanks.Count == 0)
        {
            Debug.LogError("[RankingManager] �����L���O�擾���s: �f�[�^�����݂��܂���B");

            for (int i = 0; i < rankingTextList.Count; i++)
            {
                rankingTextList[i].text = $"No.{i + 1} --- : ---";
            }
        }
        else
        {
            DisplayRanking(topRanks);
        }

        Debug.Log("[RankingManager] �����L���O�\�����������B");
        _isFetchingTopRankings = false; // �A�Ŗh�~�t���O����
    }


    private void DisplayRanking(List<(int rank, string name, int score)> rankings)
    {
        for (int i = 0; i < rankingTextList.Count; i++)
        {
            if (i < rankings.Count)
            {
                var entry = rankings[i];
                rankingTextList[i].text = $"No.{entry.rank} {entry.name} : {entry.score}";
            }
            else
            {
                rankingTextList[i].text = $"No.{i + 1} --- : ---";
            }
        }
    }
}
