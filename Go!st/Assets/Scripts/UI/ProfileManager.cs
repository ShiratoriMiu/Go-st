using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text rankText;
    [SerializeField] Text bestScoreText;

    [SerializeField] private FirebaseController firebaseController;

    private async void OnEnable()
    {
        string name = LoginController.Instance.GetUserName();
        nameText.text = $"Name : {name}";

        var bestScore = await firebaseController.OnMyRankingButtonPressed();

        rankText.text = $"Rank : {bestScore.rank.ToString()}";

        bestScoreText.text = $"BestScore : {bestScore.score.ToString()}";
    }
}
