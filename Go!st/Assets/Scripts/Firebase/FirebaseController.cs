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

                Debug.Log($"[FirebaseController] �����L���O�ۑ�JSON: {json}");
                await reference.Child("rankings").Child(uid).SetRawJsonValueAsync(json);
                Debug.Log("[FirebaseController] �����L���O�ۑ�����");
            }

            Debug.Log($"[FirebaseController] �X�R�A�ۑ�����: {newScore}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveMyScoreAsync �G���[: {e}");
        }
    }

    public async Task<List<(int rank, string name, int score)>> GetTopRankingsAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score)>();

        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in GetTopRankingsAsync.");
            return rankings;
        }

        try
        {
            var snapshot = await reference.Child("rankings")
                .OrderByChild("bestScore").LimitToLast(limit)
                .GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] �����L���O�f�[�^�����݂��܂���B");
                return rankings;
            }

            var tempList = snapshot.Children
                .Select(userEntry =>
                {
                    int score = int.TryParse(userEntry.Child("bestScore").Value?.ToString(), out int s) ? s : 0;
                    string name = userEntry.Child("displayName").Value?.ToString() ?? "NoName";
                    return (name, score);
                })
                .ToList();

            var rankedList = RankListByScore(tempList, x => x.score);

            rankings.AddRange(rankedList.Select(item => (item.rank, item.data.name, item.data.score)));
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetTopRankingsAsync �G���[: {e}");
        }

        return rankings;
    }

    public async Task<(int rank, int score)> GetMyRankingAsync()
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in GetMyRankingAsync.");
            return (-1, 0);
        }

        try
        {
            string uid = user.UserId;
            var mySnapshot = await reference.Child("rankings").Child(uid).GetValueAsync();

            if (!mySnapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] ���g�̃����L���O�G���g�������݂��܂���B");
                return (-1, 0);
            }

            int myScore = int.TryParse(mySnapshot.Child("bestScore").Value?.ToString(), out int s) ? s : 0;

            var allSnapshot = await reference.Child("rankings").GetValueAsync();

            if (!allSnapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] �����L���O�f�[�^�����݂��܂���B");
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
            Debug.LogError($"[FirebaseController] GetMyRankingAsync �G���[: {e}");
            return (-1, 0);
        }
    }

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
                Debug.Log("[FirebaseController] �����L���O�G���g�������������܂����B");

                for (int i = 0; i < 20; i++)
                {
                    var confirmSnapshot = await rankingRef.GetValueAsync();
                    if (confirmSnapshot.Exists)
                    {
                        Debug.Log("[FirebaseController] �����L���O�G���g���̔��f�m�F����");
                        return;
                    }
                    await Task.Delay(500);
                }

                Debug.LogWarning("[FirebaseController] �����L���O�G���g���̔��f�m�F�Ɏ��s���܂����B");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] EnsureRankingEntryAsync �G���[: {e}");
        }
    }

    public async Task<List<(int rank, string name, int score)>> GetTopRankingsFromServerAsync()
    {
        var rankings = new List<(int rank, string name, int score)>();

        try
        {
            var snapshot = await FirebaseDatabase.DefaultInstance.GetReference("rankings").GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning("[FirebaseController] GetTopRankingsFromServerAsync: �f�[�^�����݂��܂���B");
                return rankings;
            }

            var tempList = snapshot.Children
                .Select(entry =>
                {
                    int score = int.TryParse(entry.Child("bestScore").Value?.ToString(), out int sc) ? sc : 0;
                    string name = entry.Child("displayName").Value?.ToString() ?? "NoName";
                    return (name, score);
                })
                .ToList();

            var rankedList = RankListByScore(tempList, x => x.score);
            rankings.AddRange(rankedList.Select(item => (item.rank, item.data.name, item.data.score)));
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetTopRankingsFromServerAsync �G���[: {e}");
        }

        return rankings;
    }
}
