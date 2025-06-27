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
    // LoginController�̃C���X�^���X���烆�[�U�[�����擾����z��
    LoginController login => LoginController.Instance;

    // isFirebaseReady �� user �`�F�b�N��LoginController���ōs��
    bool IsReady => login != null && login.IsFirebaseReady && login.User != null;

    DatabaseReference reference => login?.DbReference;
    Firebase.Auth.FirebaseUser user => login?.User;

    // ? �X�R�A�ۑ��F���ȗ��� + ���ȃx�X�g + �����L���O�X�V
    public async Task SaveMyScoreAsync(int newScore)
    {
        if (!IsReady) return;

        string uid = user.UserId;
        var userRef = reference.Child("users").Child(uid);

        // �x�X�g�X�R�A�X�V�`�F�b�N
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

                Debug.Log("��������JSON: " + json);
                await reference.Child("rankings").Child(uid).SetRawJsonValueAsync(json);
                Debug.Log("�����L���O�ۑ�����");
            }
            catch (Exception e)
            {
                Debug.LogError("�����L���O�ۑ����s: " + e.Message);
            }
        }

        Debug.Log("�X�R�A�ۑ�����: " + newScore);
    }


    // ? ���������L���O�擾�i���N���j
    public async Task<List<(int rank, string name, int score)>> GetTopRankingsAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score)>();

        if (!IsReady) return rankings;

        var snapshot = await reference.Child("rankings")
            .OrderByChild("bestScore").LimitToLast(limit)
            .GetValueAsync();

        var tempList = new List<(string name, int score)>();

        // �����Ȃ̂ň�U�i�[
        foreach (var userEntry in snapshot.Children)
        {
            string name = userEntry.Child("displayName").Value?.ToString() ?? "NoName";
            int score = int.Parse(userEntry.Child("bestScore").Value?.ToString() ?? "0");
            tempList.Add((name, score));
        }

        // �~���ɕ��ёւ��ď��ʂ�����
        tempList.Sort((a, b) => b.score.CompareTo(a.score));

        for (int i = 0; i < tempList.Count; i++)
        {
            rankings.Add((i + 1, tempList[i].name, tempList[i].score));
        }

        return rankings;
    }


    // ? ���ȃx�X�g�擾�i���ʁ{�X�R�A�j
    public async Task<(int rank, int score)> GetMyRankingAsync()
    {
        if (!IsReady) return (-1, 0);

        string uid = user.UserId;

        // �����̃����L���O�f�[�^�����擾
        var mySnapshot = await reference.Child("rankings").Child(uid).GetValueAsync();
        if (!mySnapshot.Exists) return (-1, 0);

        string scoreStr = mySnapshot.Child("bestScore").Value?.ToString();
        int score = 0;
        if (!int.TryParse(scoreStr, out score)) return (-1, 0);

        // �S�̂��珇�ʂ����ׂ�
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
