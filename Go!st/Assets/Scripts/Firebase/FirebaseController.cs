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

    /// <summary>
    /// �X�R�A�ۑ��F���ȗ��� + ���ȃx�X�g�X�V + �����L���O�X�V
    /// </summary>
    public async Task SaveMyScoreAsync(int newScore)
    {
        if (!IsReady) return;

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

                Debug.Log($"�����L���O�ۑ�JSON: {json}");
                await reference.Child("rankings").Child(uid).SetRawJsonValueAsync(json);
                Debug.Log("�����L���O�ۑ�����");
            }

            Debug.Log($"�X�R�A�ۑ�����: {newScore}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveMyScoreAsync �G���[: {e}");
        }
    }

    /// <summary>
    /// ���������L���O�擾�i���N���j
    /// </summary>
    public async Task<List<(int rank, string name, int score)>> GetTopRankingsAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score)>();
        if (!IsReady) return rankings;

        try
        {
            var snapshot = await reference.Child("rankings")
                .OrderByChild("bestScore").LimitToLast(limit)
                .GetValueAsync();

            var tempList = snapshot.Children
                .Select(userEntry => (
                    name: userEntry.Child("displayName").Value?.ToString() ?? "NoName",
                    score: int.Parse(userEntry.Child("bestScore").Value?.ToString() ?? "0")
                ))
                .ToList();

            var rankedList = RankListByScore(tempList, x => x.score);

            rankings.AddRange(rankedList.Select(item => (item.rank, item.data.name, item.data.score)));
        }
        catch (Exception e)
        {
            Debug.LogError($"GetTopRankingsAsync �G���[: {e}");
        }

        return rankings;
    }

    /// <summary>
    /// �����̃����L���O�擾
    /// </summary>
    public async Task<(int rank, int score)> GetMyRankingAsync()
    {
        if (!IsReady) return (-1, 0);

        try
        {
            string uid = user.UserId;
            var mySnapshot = await reference.Child("rankings").Child(uid).GetValueAsync();

            if (!mySnapshot.Exists) return (-1, 0);

            int myScore = int.Parse(mySnapshot.Child("bestScore").Value?.ToString() ?? "0");

            var allSnapshot = await reference.Child("rankings").GetValueAsync();
            if (!allSnapshot.Exists) return (-1, myScore);

            var rankingList = allSnapshot.Children
                .Select(entry =>
                {
                    int.TryParse(entry.Child("bestScore").Value?.ToString(), out int score);
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
            Debug.LogError($"GetMyRankingAsync �G���[: {e}");
            return (-1, 0);
        }
    }

    /// <summary>
    /// �X�R�A�Ɋ�Â������ʑΉ��Ń����L���O�t�^
    /// </summary>
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

    /// <summary>
    /// �N����1��ĂсA�����L���O�G���g���𑶍݂��Ȃ���Ώ�����
    /// </summary>
    public async Task EnsureRankingEntryAsync()
    {
        if (!IsReady) return;

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
                Debug.Log("�����L���O�G���g�������������܂����B");

                // ���f�m�F�i�ő�5�񃊃g���C�j
                for (int i = 0; i < 20; i++)
                {
                    var confirmSnapshot = await rankingRef.GetValueAsync();
                    if (confirmSnapshot.Exists)
                    {
                        Debug.Log("�����L���O�G���g���̔��f�m�F����");
                        return;
                    }
                    await Task.Delay(500);
                }

                Debug.LogWarning("�����L���O�G���g���̔��f�m�F�Ɏ��s���܂����B");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"EnsureRankingEntryAsync �G���[: {e}");
        }
    }
}
