using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private FirebaseController firebaseController;

    [Header("ランキングUI設定")]
    [SerializeField] private Transform rankingContainer; // VerticalLayoutGroup / GridLayoutGroup 推奨
    [SerializeField] private GameObject rankingTextBGPrefab;
    [SerializeField] private int maxDisplayCount = 100;

    private List<Text> rankingTextList = new List<Text>();
    private Task initializationTask;

    private bool _isFetchingTopRankings = false; // 連打防止用フラグ

    private async void Start()
    {
        Debug.Log("[RankingManager] Firebase準備完了待機中...");
        await LoginController.Instance.WaitForFirebaseReadyAsync();
        Debug.Log("[RankingManager] Firebase準備完了。ランキングエントリ初期化開始...");

        initializationTask = firebaseController.EnsureRankingEntryAsync();
        await initializationTask;

        Debug.Log("[RankingManager] ランキングエントリ初期化完了。");

        GenerateRankingTextUI();
    }

    private void GenerateRankingTextUI()
    {
        // すでに生成済みのUIを削除
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
        if (_isFetchingTopRankings) return; // 連打防止
        _isFetchingTopRankings = true;

        Debug.Log("[RankingManager] ランキングボタン押下。初期化完了待機開始...");

        if (initializationTask != null)
        {
            await initializationTask;
            initializationTask = null;
        }

        Debug.Log("[RankingManager] 初期化完了。ランキング取得開始...");

        const int MaxRetryCount = 10; // 最大10回再取得（約5秒）
        int retryCount = 0;
        List<(int rank, string name, int score)> topRanks = null;

        while (retryCount < MaxRetryCount)
        {
            topRanks = await firebaseController.GetTopRankingsFromServerAsync(maxDisplayCount);

            if (topRanks.Count > 0)
            {
                Debug.Log($"[RankingManager] ランキング取得成功（試行回数: {retryCount + 1}） 件数: {topRanks.Count}");
                break;
            }
            else
            {
                Debug.LogWarning($"[RankingManager] ランキング取得結果が0件、再取得試行中 ({retryCount + 1}/{MaxRetryCount})...");
                await Task.Delay(500); // 0.5秒待機
            }

            retryCount++;
        }

        if (topRanks == null || topRanks.Count == 0)
        {
            Debug.LogError("[RankingManager] ランキング取得失敗: データが存在しません。");

            for (int i = 0; i < rankingTextList.Count; i++)
            {
                rankingTextList[i].text = $"No.{i + 1} --- : ---";
            }
        }
        else
        {
            DisplayRanking(topRanks);
        }

        Debug.Log("[RankingManager] ランキング表示処理完了。");
        _isFetchingTopRankings = false; // 連打防止フラグ解除
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
