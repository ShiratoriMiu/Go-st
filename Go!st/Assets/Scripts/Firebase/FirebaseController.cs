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
                Debug.Log("Firebase �ˑ��֌W�`�F�b�N����");

                // FirebaseApp ������
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // �����I��DatabaseURL�t���ŃC���X�^���X�擾
                //var db = FirebaseDatabase.GetInstance("https://go-st-63ded-default-rtdb.firebaseio.com/");
                //reference = db.RootReference;
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseReady = true;

                Debug.Log("Firebase �������������܂���");
            }
            else
            {
                Debug.LogError("Firebase ���������s: " + task.Result);
            }
        });
    }


    // Firebase �ɃX�R�A��񓯊��ۑ�
    public async Task SaveScoreAsync(int score)
    {
        if (!isFirebaseReady)
        {
            Debug.LogWarning("Firebase���܂�����������Ă��܂���B");
            return;
        }

        string key = reference.Child("scores").Push().Key;
        try
        {
            await reference.Child("scores").Child(key).SetValueAsync(score);
            Debug.Log("�X�R�A���M����: " + score);
        }
        catch (Exception e)
        {
            Debug.LogError("�X�R�A���M���s: " + e);
        }
    }

    // Firebase �����ʃX�R�A���擾
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
                Debug.Log($"�擾�f�[�^ Key={scoreSnapshot.Key}, Value={scoreSnapshot.Value} (�^={scoreSnapshot.Value?.GetType()})");
                int score = Convert.ToInt32(scoreSnapshot.Value);
                topScores.Add(score);
            }

            // �������ɕ��ёւ�
            topScores.Sort((a, b) => b.CompareTo(a));
        }
        catch (Exception e)
        {
            Debug.LogError("�X�R�A�擾���s: " + e);
        }

        return topScores;
    }
}