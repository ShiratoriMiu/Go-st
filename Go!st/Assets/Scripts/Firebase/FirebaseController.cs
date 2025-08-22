using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class RankingData
{
    public string displayName;
    public int bestScore;

    public RankingData(string name, int score)
    {
        displayName = name;
        bestScore = score;
    }
}

[Serializable]
public class ScoreEntry
{
    public int score;
    public string timestamp;

    public ScoreEntry(int score)
    {
        this.score = score;
        this.timestamp = DateTime.UtcNow.ToString("o");
    }
}

public class FirebaseController : MonoBehaviour
{
    private LoginController login => LoginController.Instance;
    private bool IsReady => login != null && login.IsFirebaseReady && login.User != null;
    private DatabaseReference reference => login?.DbReference;
    private Firebase.Auth.FirebaseUser user => login?.User;

    // ------------------------------
    // ランキング保存
    // ------------------------------
    public async Task SaveMyScoreAsync(int newScore)
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in SaveMyScoreAsync.");
            return;
        }

        try
        {
            string uid = user.UserId;
            var userRef = reference.Child("users").Child(uid);

            var bestSnapshot = await userRef.Child("bestScore").GetValueAsync();
            int best = bestSnapshot.Exists ? int.Parse(bestSnapshot.Value.ToString()) : 0;

            if (newScore > best)
            {
                await userRef.Child("bestScore").SetValueAsync(newScore);

                string displayName = user.DisplayName ?? "NoName";
                var data = new RankingData(displayName, newScore);
                string json = JsonUtility.ToJson(data);

                await reference.Child("rankings").Child(uid).SetRawJsonValueAsync(json);
                Debug.Log($"[FirebaseController] ランキング保存成功: {json}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveMyScoreAsync エラー: {e}");
        }
    }

    // ------------------------------
    // 上位ランキング取得（サーバー直）
    // ------------------------------
    public async Task<List<(int rank, string name, int score)>> GetTopRankingsFromServerAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score)>();

        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in GetTopRankingsFromServerAsync.");
            return rankings;
        }

        try
        {
            var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("rankings").GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] ランキングデータが存在しません。");
                return rankings;
            }

            var tempList = snapshot.Children
                .Select(entry =>
                {
                    int score = int.TryParse(entry.Child("bestScore").Value?.ToString(), out int sc) ? sc : 0;
                    string name = entry.Child("displayName").Value?.ToString() ?? "NoName";
                    return (name, score);
                })
                .OrderByDescending(x => x.score)
                .Take(limit)
                .ToList();

            var rankedList = RankListByScore(tempList, x => x.score);

            rankings.AddRange(rankedList.Select(item => (item.rank, item.data.name, item.data.score)));
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetTopRankingsFromServerAsync エラー: {e}");
        }

        return rankings;
    }

    private bool _isFetchingRanking = false;

    public async Task<(int rank, int score)> OnMyRankingButtonPressed()
    {
        if (_isFetchingRanking) return (-1, 0); // 連打防止

        _isFetchingRanking = true;

        try
        {
            await login.WaitForFirebaseReadyAsync();

            var (rank, score) = await GetMyRankingAsync();

            if (rank == -1)
            {
                Debug.LogWarning("ランキング取得失敗またはデータなし");
            }
            else
            {
                Debug.Log($"自分の順位: {rank}, スコア: {score}");
            }

            return (rank, score);
        }
        catch (Exception e)
        {
            Debug.LogError($"ランキング取得時にエラー: {e}");
            return (-1, 0);
        }
        finally
        {
            _isFetchingRanking = false;
        }
    }


    // ------------------------------
    // 自分の順位取得（サーバー直）
    // ------------------------------
    public async Task<(int rank, int score)> GetMyRankingAsync()
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in GetMyRankingAsync.");
            return (-1, 0);
        }

        string uid = user.UserId;

        try
        {
            var mySnapshot = await FirebaseDatabase.DefaultInstance
                .GetReference("rankings")
                .Child(uid)
                .GetValueAsync();

            if (!mySnapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] 自身のランキングエントリが存在しません。");
                return (-1, 0);
            }

            int myScore = int.TryParse(mySnapshot.Child("bestScore").Value?.ToString(), out int s) ? s : 0;

            var allSnapshot = await FirebaseDatabase.DefaultInstance
                .GetReference("rankings")
                .GetValueAsync();

            if (!allSnapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] ランキングデータが存在しません。");
                return (-1, myScore);
            }

            var rankingList = allSnapshot.Children
                .Select(entry =>
                {
                    int score = int.TryParse(entry.Child("bestScore").Value?.ToString(), out int sc) ? sc : 0;
                    return (uid: entry.Key, score);
                })
                .ToList();

            var rankedList = RankListByScore(rankingList, x => x.score);

            foreach (var item in rankedList)
            {
                if (item.data.uid == uid)
                {
                    return (item.rank, myScore);
                }
            }

            return (-1, myScore);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetMyRankingAsync エラー: {e}");
            return (-1, 0);
        }
    }

    // ------------------------------
    // 順位計算共通関数
    // ------------------------------
    public static List<(int rank, T data)> RankListByScore<T>(List<T> dataList, Func<T, int> getScore)
    {
        var sortedList = dataList.OrderByDescending(getScore).ToList();
        var rankedList = new List<(int rank, T data)>();

        int rank = 1;
        int actualRank = 1;
        int? prevScore = null;

        foreach (var item in sortedList)
        {
            int score = getScore(item);
            if (prevScore != score)
            {
                actualRank = rank;
                prevScore = score;
            }

            rankedList.Add((actualRank, item));
            rank++;
        }

        return rankedList;
    }

    // ------------------------------
    // ランキングエントリ初期化
    // ------------------------------
    public async Task EnsureRankingEntryAsync()
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in EnsureRankingEntryAsync.");
            return;
        }

        try
        {
            string uid = user.UserId;
            var rankingRef = reference.Child("rankings").Child(uid);
            var snapshot = await rankingRef.GetValueAsync();

            if (!snapshot.Exists)
            {
                string name = user.DisplayName ?? "NoName";
                var data = new RankingData(name, 0);
                string json = JsonUtility.ToJson(data);

                await rankingRef.SetRawJsonValueAsync(json);
                Debug.Log("[FirebaseController] ランキングエントリを初期化しました。");

                for (int i = 0; i < 20; i++)
                {
                    var confirmSnapshot = await rankingRef.GetValueAsync();
                    if (confirmSnapshot.Exists && confirmSnapshot.Child("bestScore").Exists)
                    {
                        Debug.Log("[FirebaseController] ランキングエントリの反映確認完了");
                        return;
                    }
                    await Task.Delay(500);
                }

                Debug.LogWarning("[FirebaseController] ランキングエントリの反映確認に失敗しました。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] EnsureRankingEntryAsync エラー: {e}");
        }
    }

    // ------------------------------
    // コイン保存（加算・減算可）
    // ------------------------------
    public async Task SaveMyCoinsAsync(int earnedCoins)
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in SaveMyCoinsAsync.");
            return;
        }

        try
        {
            string uid = user.UserId;
            var userRef = reference.Child("users").Child(uid).Child("coins");

            var coinSnapshot = await userRef.GetValueAsync();
            int currentCoins = coinSnapshot.Exists ? int.Parse(coinSnapshot.Value.ToString()) : 0;

            int newTotal = Mathf.Max(0, currentCoins + earnedCoins);
            await userRef.SetValueAsync(newTotal);

            Debug.Log($"[FirebaseController] コイン保存完了: {(earnedCoins >= 0 ? "+" : "")}{earnedCoins} → 合計 {newTotal}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveMyCoinsAsync エラー: {e}");
        }
    }

    // ------------------------------
    // コイン取得
    // ------------------------------
    public async Task<int> GetMyCoinsAsync()
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in GetMyCoinsAsync.");
            return 0;
        }

        try
        {
            string uid = user.UserId;
            var userRef = reference.Child("users").Child(uid).Child("coins");

            var coinSnapshot = await userRef.GetValueAsync();

            if (coinSnapshot.Exists)
            {
                int coins = int.Parse(coinSnapshot.Value.ToString());
                Debug.Log($"[FirebaseController] 所持コイン: {coins}");
                return coins;
            }
            else
            {
                Debug.Log("[FirebaseController] コインデータが存在しないため 0 を返します。");
                return 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetMyCoinsAsync エラー: {e}");
            return 0;
        }
    }
}
