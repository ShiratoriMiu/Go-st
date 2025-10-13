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

[System.Serializable]
public class PlayerIconDataFirebase
{
    public string name;
    public string style;

    public PlayerIconDataFirebase() { }

    public PlayerIconDataFirebase(PlayerIconData icon)
    {
        name = icon.name;
        style = icon.style.ToString();
    }
}

[Serializable]
class PlayerIconDataWrapper
{
    public List<PlayerIconDataFirebase> icons;
}

public class FirebaseController : MonoBehaviour
{
    private LoginController login => LoginController.Instance;
    private bool IsReady => login != null && login.IsFirebaseReady && login.User != null;
    private DatabaseReference reference => login?.DbReference;
    private Firebase.Auth.FirebaseUser user => login?.User;

    // ------------------------------
    // �����L���O�ۑ�
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

                string displayName = user.DisplayName ?? "Player";
                var rankingRef = reference.Child("rankings").Child(uid);

                // �����̃����L���O�f�[�^�擾
                var rankingSnapshot = await rankingRef.GetValueAsync();

                List<Dictionary<string, object>> equippedIcons = new();

                if (rankingSnapshot.Exists && rankingSnapshot.Child("equippedIcons").Exists)
                {
                    // ������equippedIcons��ێ�
                    foreach (var iconSnap in rankingSnapshot.Child("equippedIcons").Children)
                    {
                        equippedIcons.Add(new Dictionary<string, object>
                    {
                        { "name", iconSnap.Child("name").Value?.ToString() },
                        { "style", iconSnap.Child("style").Value?.ToString() }
                    });
                    }
                }

                // �������ĕۑ�
                var data = new Dictionary<string, object>
            {
                { "displayName", displayName },
                { "bestScore", newScore },
                { "equippedIcons", equippedIcons }
            };

                await rankingRef.SetValueAsync(data);
                Debug.Log($"[FirebaseController] �X�R�A�X�V�{�����ێ��Ń����L���O�ۑ�����: {displayName}, {newScore}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveMyScoreAsync �G���[: {e}");
        }
    }


    // ------------------------------
    // ��ʃ����L���O�擾�i�T�[�o�[���j
    // ------------------------------
    public async Task<List<(int rank, string name, int score, List<PlayerIconData> icons)>> GetTopRankingsFromServerAsync(int limit = 100)
    {
        var rankings = new List<(int rank, string name, int score, List<PlayerIconData> icons)>();

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
                Debug.LogWarning("[FirebaseController] �����L���O�f�[�^�����݂��܂���B");
                return rankings;
            }

            // ��U���ׂẴG���g���[��ǂݍ���
            var tempList = new List<(string name, int score, List<PlayerIconData> icons)>();

            foreach (var entry in snapshot.Children)
            {
                string name = entry.Child("displayName").Value?.ToString() ?? "NoName";
                int score = int.TryParse(entry.Child("bestScore").Value?.ToString(), out int sc) ? sc : 0;

                // --- �A�C�R�����X�g��ǂݎ�� ---
                var icons = new List<PlayerIconData>();
                var iconsNode = entry.Child("equippedIcons");

                if (iconsNode.Exists)
                {
                    foreach (var icon in iconsNode.Children)
                    {
                        string iconName = icon.Child("name").Value?.ToString();
                        string styleStr = icon.Child("style").Value?.ToString();

                        if (!string.IsNullOrEmpty(iconName) && Enum.TryParse(styleStr, true, out PlayerIconStyle style))
                        {
                            icons.Add(new PlayerIconData(iconName, style));
                        }
                    }
                }

                tempList.Add((name, score, icons));
            }

            // �X�R�A���Ƀ\�[�g
            var rankedList = RankListByScore(tempList, x => x.score);

            // rank�t���Ń��X�g�ɒǉ�
            rankings.AddRange(rankedList.Select(item => (
                item.rank,
                item.data.name,
                item.data.score,
                item.data.icons
            )));
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetTopRankingsFromServerAsync �G���[: {e}");
        }

        return rankings;
    }

    private bool _isFetchingRanking = false;

    public async Task<(int rank, int score)> OnMyRankingButtonPressed()
    {
        if (_isFetchingRanking) return (-1, 0); // �A�Ŗh�~

        _isFetchingRanking = true;

        try
        {
            await login.WaitForFirebaseReadyAsync();

            var (rank, score) = await GetMyRankingAsync();

            if (rank == -1)
            {
                Debug.LogWarning("�����L���O�擾���s�܂��̓f�[�^�Ȃ�");
            }
            else
            {
                Debug.Log($"�����̏���: {rank}, �X�R�A: {score}");
            }

            return (rank, score);
        }
        catch (Exception e)
        {
            Debug.LogError($"�����L���O�擾���ɃG���[: {e}");
            return (-1, 0);
        }
        finally
        {
            _isFetchingRanking = false;
        }
    }


    // ------------------------------
    // �����̏��ʎ擾�i�T�[�o�[���j
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
                Debug.LogWarning("[FirebaseController] ���g�̃����L���O�G���g�������݂��܂���B");
                return (-1, 0);
            }

            int myScore = int.TryParse(mySnapshot.Child("bestScore").Value?.ToString(), out int s) ? s : 0;

            var allSnapshot = await FirebaseDatabase.DefaultInstance
                .GetReference("rankings")
                .GetValueAsync();

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

    // ------------------------------
    // ���ʌv�Z���ʊ֐�
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
    // �����L���O�G���g��������
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
                Debug.Log("[FirebaseController] �����L���O�G���g�������������܂����B");

                for (int i = 0; i < 20; i++)
                {
                    var confirmSnapshot = await rankingRef.GetValueAsync();
                    if (confirmSnapshot.Exists && confirmSnapshot.Child("bestScore").Exists)
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

    // ------------------------------
    // �R�C���ۑ��i���Z�E���Z�j
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

            Debug.Log($"[FirebaseController] �R�C���ۑ�����: {(earnedCoins >= 0 ? "+" : "")}{earnedCoins} �� ���v {newTotal}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveMyCoinsAsync �G���[: {e}");
        }
    }

    // ------------------------------
    // �R�C���擾
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
                Debug.Log($"[FirebaseController] �����R�C��: {coins}");
                return coins;
            }
            else
            {
                Debug.Log("[FirebaseController] �R�C���f�[�^�����݂��Ȃ����� 0 ��Ԃ��܂��B");
                return 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] GetMyCoinsAsync �G���[: {e}");
            return 0;
        }
    }

    // ------------------------------
    // �����v���C���[�A�C�R���ۑ�
    // ------------------------------
    public async Task SaveEquippedIconsAsync(List<PlayerIconData> equippedIcons)
    {
        if (!IsReady)
        {
            Debug.LogWarning("[FirebaseController] Firebase not ready in SaveEquippedIconsAsync.");
            return;
        }

        try
        {
            string uid = user.UserId;
            var rankingRef = reference.Child("rankings").Child(uid);
            var snapshot = await rankingRef.GetValueAsync();

            string displayName = user.DisplayName ?? "Player";
            int bestScore = 0;

            if (snapshot.Exists)
            {
                displayName = snapshot.Child("displayName").Value?.ToString() ?? displayName;
                int.TryParse(snapshot.Child("bestScore").Value?.ToString(), out bestScore);
            }

            // �����L���O�{�̃f�[�^�idisplayName��score�j
            await rankingRef.Child("displayName").SetValueAsync(displayName);
            await rankingRef.Child("bestScore").SetValueAsync(bestScore);

            // �A�C�R���f�[�^����
            var equippedIconsRef = rankingRef.Child("equippedIcons");
            await equippedIconsRef.RemoveValueAsync(); // �����f�[�^���N���A

            for (int i = 0; i < equippedIcons.Count; i++)
            {
                var icon = equippedIcons[i];
                var iconRef = equippedIconsRef.Child(i.ToString());
                await iconRef.Child("name").SetValueAsync(icon.name);
                await iconRef.Child("style").SetValueAsync(icon.style.ToString());
            }

            Debug.Log("[FirebaseController] equippedIcons���ʃm�[�h�ŕۑ����܂����B");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseController] SaveEquippedIconsAsync �G���[: {e}");
        }
    }



    // ------------------------------
    // �����v���C���[�A�C�R���ۑ�
    // ------------------------------

    public async Task EquipPlayerIconAsync(PlayerIconData newIcon)
    {
        // Firebase �ۑ�
        var equippedIcons = SaveManager.LoadEquippedPlayerIcons();
        await SaveEquippedIconsAsync(equippedIcons);
    }
}
