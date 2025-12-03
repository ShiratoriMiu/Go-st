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
    [SerializeField] private ScrollRect rankingScrollRect;

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
        if (rankingContainer == null) return;

        // すでに生成済みのUIを削除
        foreach (Transform child in rankingContainer)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                // エディタモード中に呼ばれた場合はこちらを使用
                DestroyImmediate(child.gameObject);
            }
        }
        rankingTextList.Clear();

        for (int i = 0; i < maxDisplayCount; i++)
        {
            GameObject bg = Instantiate(rankingTextBGPrefab, rankingContainer);
            Text rankingText = bg.GetComponentInChildren<Text>();
            rankingText.text = $"{i + 1}.  --- : ---";
            rankingTextList.Add(rankingText);
        }
    }

    public async void OnRankingButtonClicked()
    {
        if (_isFetchingTopRankings) return; // 連打防止
        // スクロール位置を一番上に戻す
        if (rankingScrollRect != null)
            rankingScrollRect.verticalNormalizedPosition = 1f;
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
        List<(int rank, string uid, string name, int score, List<PlayerIconData> icons)> topRanks = null;

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
                rankingTextList[i].text = $"{i + 1}. --- : ---";
            }
        }
        else
        {
            DisplayRanking(topRanks);
        }

        Debug.Log("[RankingManager] ランキング表示処理完了。");
        _isFetchingTopRankings = false; // 連打防止フラグ解除
    }

    private void DisplayRanking(List<(int rank, string uid, string name, int score, List<PlayerIconData> icons)> rankings)
    {
        string myUid = LoginController.Instance.User.UserId;

        for (int i = 0; i < rankingTextList.Count; i++)
        {
            var text = rankingTextList[i];
            var entryTransform = text.transform.parent;

            // ←ここを複数取得に変更
            var myRankObjs = FindMyRankObjects(entryTransform);

            if (i < rankings.Count)
            {
                var entry = rankings[i];
                text.text = $"{entry.rank}. {entry.name} : {entry.score}";

                bool isMe = entry.uid == myUid;

                // 複数 MyRank オブジェクトを一括制御
                foreach (var obj in myRankObjs)
                    obj.SetActive(isMe);

                // アイコン処理などはそのまま
                // --- アイコン反映処理 ---
                Transform bgTransform = rankingTextList[i].transform.parent; // 親のRankingTextBGPrefab
                PlayerIconParts playerIconParts = bgTransform.GetComponent<PlayerIconParts>();
                Image iconImage = playerIconParts.iconImage;
                Image iconBG = playerIconParts.iconBGImage;

                if (iconImage != null && entry.icons != null && entry.icons.Count > 0)
                {
                    // まず非表示
                    iconImage.enabled = false;
                    if (iconBG != null) iconBG.enabled = false;

                    // 各アイコンデータを読み込み
                    foreach (var iconData in entry.icons)
                    {
                        Sprite sprite = Resources.Load<Sprite>($"PlayerIcon/{iconData.name}");
                        Debug.Log(iconData.name);
                        if (sprite == null)
                        {
                            Debug.LogWarning($"[RankingManager] アイコン '{iconData.name}' が Resources/PlayerIcon に見つかりません。");
                            continue;
                        }

                        if (iconData.style == PlayerIconStyle.Chara)
                        {
                            if (iconImage != null)
                            {
                                iconImage.sprite = sprite;
                                iconImage.enabled = true;
                            }
                        }
                        else
                        {
                            if (iconBG != null)
                            {
                                iconBG.sprite = sprite;
                                iconBG.enabled = true;
                            }
                        }
                    }
                }
            }
            else
            {
                text.text = $"{i + 1}. --- : ---";

                // データのない行は全部OFF
                foreach (var obj in myRankObjs)
                    obj.SetActive(false);
            }
        }
    }

    private List<GameObject> FindMyRankObjects(Transform parent)
    {
        List<GameObject> list = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child.CompareTag("MyRank"))
                list.Add(child.gameObject);
        }
        return list;
    }
}
