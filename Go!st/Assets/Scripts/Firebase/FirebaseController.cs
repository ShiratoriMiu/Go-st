using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseController : MonoBehaviour
{
    private DatabaseReference reference;
    bool isFirebaseReady = false;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase 依存関係チェック成功");

                // FirebaseApp 初期化
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // 明示的にDatabaseURL付きでインスタンス取得
                //var db = FirebaseDatabase.GetInstance("https://go-st-63ded-default-rtdb.firebaseio.com/");
                //reference = db.RootReference;
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseReady = true;

                Debug.Log("Firebase 初期化完了しました");
            }
            else
            {
                Debug.LogError("Firebase 初期化失敗: " + task.Result);
            }
        });
    }


    // Firebase にスコアを非同期保存
    public async Task SaveScoreAsync(int score)
    {
        if (!isFirebaseReady)
        {
            Debug.LogWarning("Firebaseがまだ初期化されていません。");
            return;
        }

        string key = reference.Child("scores").Push().Key;
        try
        {
            await reference.Child("scores").Child(key).SetValueAsync(score);
            Debug.Log("スコア送信完了: " + score);
        }
        catch (Exception e)
        {
            Debug.LogError("スコア送信失敗: " + e);
        }
    }

    // Firebase から上位スコアを取得
    public async Task<List<int>> GetTopScoresAsync()
    {
        List<int> topScores = new List<int>();

        try
        {
            DataSnapshot snapshot = await reference.Child("scores")
                                                   .OrderByValue()
                                                   .LimitToLast(100)
                                                   .GetValueAsync();

            foreach (DataSnapshot scoreSnapshot in snapshot.Children)
            {
                Debug.Log($"取得データ Key={scoreSnapshot.Key}, Value={scoreSnapshot.Value} (型={scoreSnapshot.Value?.GetType()})");
                int score = Convert.ToInt32(scoreSnapshot.Value);
                topScores.Add(score);
            }

            // 高い順に並び替え
            topScores.Sort((a, b) => b.CompareTo(a));
        }
        catch (Exception e)
        {
            Debug.LogError("スコア取得失敗: " + e);
        }

        return topScores;
    }
}