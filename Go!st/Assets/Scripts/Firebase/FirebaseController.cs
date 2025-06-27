using Firebase.Database;
using System;
using System.Collections.Generic;
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
    // LoginControllerのインスタンスからユーザー情報を取得する想定
    LoginController login => LoginController.Instance;

    // isFirebaseReady と user チェックをLoginController側で行う
    bool IsReady => login != null && login.IsFirebaseReady && login.User != null;

    DatabaseReference reference => login?.DbReference;
    Firebase.Auth.FirebaseUser user => login?.User;

    // ? スコア保存：自己履歴 + 自己ベスト + ランキング更新
    public async Task SaveMyScoreAsync(int newScore)
    {
        if (!IsReady) return;

        string uid = user.UserId;
        var userRef = reference.Child("users").Child(uid);

        // ベストスコア更新チェック
        var bestSnapshot = await userRef.Child("bestScore").GetValueAsync();
        int best = 0;
        if (bestSnapshot.Exists)
            int.TryParse(bestSnapshot.Value.ToString(), out best);

        if (newScore > best)
        {
            await userRef.Child("bestScore").SetValueAsync(newScore);

            try
            {
                string displayName = user.DisplayName ?? "NoName";
                var data = new RankingData(displayName, newScore);
                string json = JsonUtility.ToJson(data);

                Debug.Log("書き込むJSON: " + json);
                await reference.Child("rankings").Child(uid).SetRawJsonValueAsync(json);
                Debug.Log("ランキング保存成功");
            }
            catch (Exception e)
            {
                Debug.LogError("ランキング保存失敗: " + e.Message);
            }
        }

        Debug.Log("スコア保存完了: " + newScore);
    }


    // ? 総合ランキング取得（上位N件）
    public async Task<List<(int rank, string name, int score)>> GetTopRankingsAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score)>();

        if (!IsReady) return rankings;

        var snapshot = await reference.Child("rankings")
            .OrderByChild("bestScore").LimitToLast(limit)
            .GetValueAsync();

        var tempList = new List<(string name, int score)>();

        // 昇順なので一旦格納
        foreach (var userEntry in snapshot.Children)
        {
            string name = userEntry.Child("displayName").Value?.ToString() ?? "NoName";
            int score = int.Parse(userEntry.Child("bestScore").Value?.ToString() ?? "0");
            tempList.Add((name, score));
        }

        // 降順に並び替えて順位をつける
        tempList.Sort((a, b) => b.score.CompareTo(a.score));

        for (int i = 0; i < tempList.Count; i++)
        {
            rankings.Add((i + 1, tempList[i].name, tempList[i].score));
        }

        return rankings;
    }


    // ? 自己ベスト取得（順位＋スコア）
    public async Task<(int rank, int score)> GetMyRankingAsync()
    {
        if (!IsReady) return (-1, 0);

        string uid = user.UserId;

        // 自分のランキングデータだけ取得
        var mySnapshot = await reference.Child("rankings").Child(uid).GetValueAsync();
        if (!mySnapshot.Exists) return (-1, 0);

        string scoreStr = mySnapshot.Child("bestScore").Value?.ToString();
        int score = 0;
        if (!int.TryParse(scoreStr, out score)) return (-1, 0);

        // 全体から順位も調べる
        var allSnapshot = await reference.Child("rankings").OrderByChild("bestScore").GetValueAsync();
        int rank = 1;

        foreach (var entry in allSnapshot.Children)
        {
            if (entry.Key == uid) break;
            rank++;
        }

        return (rank, score);
    }
}
