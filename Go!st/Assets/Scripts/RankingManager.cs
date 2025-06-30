using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    [Header("Firebase Controller")]
    [SerializeField] private FirebaseController firebaseController;

    [Header("ランキング表示設定")]
    [SerializeField] private GameObject rankingTextBase;
    [SerializeField] private GameObject rankingTextPrefab;
    [SerializeField] private int rankingDisplayNum = 100;

    private readonly List<Text> rankingTextList = new List<Text>();
    private Task initializationTask;
    private Task firebaseReadyTask;

    private async void Start()
    {
        GenerateRankingTextUI();

        Debug.Log("[RankingManager] Firebase準備完了待機中...");
        firebaseReadyTask = LoginController.Instance.WaitForFirebaseReadyAsync();
        await firebaseReadyTask;
        Debug.Log("[RankingManager] Firebase準備完了。ランキング初期化開始...");

        initializationTask = InitializeRankingEntryAsync();
        await initializationTask;
        Debug.Log("[RankingManager] ランキング初期化完了。");
    }


    /// <summary>
    /// 指定数分のランキング表示用Textオブジェクトを生成
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
    /// 起動時ランキングエントリ初期化
    /// </summary>
    private async Task InitializeRankingEntryAsync()
    {
        if (firebaseController == null)
        {
            Debug.LogError("[RankingManager] FirebaseControllerがセットされていません");
            return;
        }

        await firebaseController.EnsureRankingEntryAsync();
    }

    /// <summary>
    /// ランキングボタンが押されたときに呼ばれる
    /// </summary>
    public async void OnRankingButtonClicked()
    {
        Debug.Log("[RankingManager] ランキングボタン押下。初期化完了待機開始...");

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

        Debug.Log("[RankingManager] 初期化完了。ランキング取得開始...");

        var topRanks = await firebaseController.GetTopRankingsAsync();
        Debug.Log($"取得したランキング数: {topRanks.Count}");
        DisplayRanking(topRanks);

        Debug.Log("[RankingManager] ランキング表示完了。");
    }


    /// <summary>
    /// ランキング情報をUIに表示
    /// </summary>
    private void DisplayRanking(List<(int rank, string name, int score)> rankings)
    {
        int displayCount = Mathf.Min(rankings.Count, rankingDisplayNum);

        for (int i = 0; i < displayCount; i++)
        {
            rankingTextList[i].text = $"{rankings[i].rank}位: {rankings[i].name}: {rankings[i].score}";
        }

        for (int i = displayCount; i < rankingDisplayNum; i++)
        {
            rankingTextList[i].text = string.Empty;
        }
    }
}
