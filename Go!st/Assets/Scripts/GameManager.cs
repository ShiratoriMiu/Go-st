using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking, SkinChange }
    public GameState state;

    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public LevelManager levelManager;

    public int enemiesDefeated = 0;
    public float survivalTime = 0f;

    [Header("Game Time Settings")]
    [SerializeField] private float maxTimeLimit = 60f; // �ő吧�����ԁi�C���X�y�N�^�[�Őݒ�j

    [Header("UI")]
    [SerializeField] private Text scoreText;           // �X�R�A�\���p
    [SerializeField] private Text remainingTimeText;   // �c�莞�ԕ\���p
    [SerializeField] private Text rankingText;         //�����L���O�\���p

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject skinChangePanel;

    [SerializeField] private float resetSelectPlayerRotateLerpSpeed = 1;

    [SerializeField] private FirebaseController firebaseController;

    CenterToGrayEffect centerToGrayEffect;

    public GameObject selectPlayer { get; private set; }

    private int finalScore;

    //�f�o�b�O�p
    private float initmaxTimeLimit;

    //frebase�r���h���o���Ȃ��̂ňꎞ�I��csv�ɕύX
    //[SerializeField] ScoreManager scoreManager;

    private void Start()
    {
        InitGame();
        SelectOnUI(titlePanel);
        centerToGrayEffect = Camera.main.GetComponent<CenterToGrayEffect>();
        initmaxTimeLimit = maxTimeLimit;
    }

    private void Update()
    {
        if (state == GameState.Game)
        {
            survivalTime += Time.deltaTime;

            float remainingTime = Mathf.Max(0f, maxTimeLimit - survivalTime);
            UpdateTimeUI(remainingTime);

            if (remainingTime <= 0f)
            {
                EndGame(); // �������Ԃ��s������Q�[���I��
            }
        }
    }

    public void InitGame()
    {
        levelManager.LoadLevelData();
        ResetGame(); // ������܂߂Ė��񏉊���
    }

    public void StartGame()
    {
        state = GameState.Game;
        SelectOnUI(gamePanel);
        levelManager.InitializeLevel();
        enemyManager.SetEnemyTypesByLevelData(levelManager.levelDataDict[1]);
        playerManager.InitializePlayer(Vector3.zero);
        enemyManager.StartSpawning();
    }

    void SelectOnUI(GameObject _obj)
    {
        // UI��S�Ĕ�\���ɂ���
        titlePanel.SetActive(false);
        gamePanel.SetActive(false);
        scorePanel.SetActive(false);
        rankingPanel.SetActive(false);
        skinChangePanel.SetActive(false);

        // �����œn���ꂽUI�I�u�W�F�N�g�݂̂�\��
        _obj.SetActive(true);
    }


    private void UpdateTimeUI(float time)
    {
        // ���Ԃ𕪂ƕb�ɕϊ�
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.CeilToInt(time % 60);

        // �t�H�[�}�b�g���ĕ\���i2���\���j
        remainingTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public void AddEnemiesDefeatedNum()
    {
        enemiesDefeated++;
    }

    public int CalculateScore()
    {
        finalScore = enemiesDefeated + Mathf.FloorToInt(survivalTime);
        return finalScore;
    }

    public async void ShowScore()
    {
        int score = CalculateScore();
        //await scoreManager.WriteScoreDataAsync(score);
        await firebaseController.SaveScoreAsync(score);
        scoreText.text = $"Score : {score}";
    }


    public async void ShowRanking()
    {
        List<int> scores = await firebaseController.GetTopScoresAsync();
        //List<int> scores = scoreManager.LoadScoreData();

        // �~���ɕ��בւ��i���X�R�A���j
        scores = scores.OrderByDescending(score => score).ToList();

        // �ő�5���܂ŕ\���A����Ȃ��ꍇ��0�Ŗ��߂�
        while (scores.Count < 5)
        {
            scores.Add(0);
        }

        string rankingResult = "";
        for (int i = 0; i < 5; i++)  // �K��5���\��
        {
            rankingResult += $"{i + 1}��: {scores[i]}\n";
        }

        rankingText.text = rankingResult;
        SelectOnUI(rankingPanel);
    }

    public void EndGame()
    {
        if (state != GameState.Game) return;

        state = GameState.Score;
        SelectOnUI(scorePanel);
        enemyManager.StopSpawning();
        ShowScore();
        remainingTimeText.text = "00 : 00";
        centerToGrayEffect.ResetGrey();
    }

    public void ScoreToRanking()
    {
        ShowRanking();
    }

    public void RankingToTitle()
    {
        state = GameState.Title;
        SelectOnUI(titlePanel);
        ResetGame();
    }

    public void SkinChangeToTitle()
    {
        state = GameState.Title;
        StartCoroutine(ResetSelectPlayerRotate());
        SelectOnUI(titlePanel);
    }

    IEnumerator ResetSelectPlayerRotate()
    {
        Quaternion targetRotation = Quaternion.identity;

        while (Quaternion.Angle(selectPlayer.transform.rotation, targetRotation) > 0.1f)
        {
            selectPlayer.transform.rotation = Quaternion.Lerp(
                selectPlayer.transform.rotation,
                targetRotation,
                Time.deltaTime * resetSelectPlayerRotateLerpSpeed
            );

            yield return null;
        }

        selectPlayer.transform.rotation = targetRotation;
    }

    private void ResetGame()
    {
        enemiesDefeated = 0;
        survivalTime = 0f;
        scoreText.text = "";
        remainingTimeText.text = $"{Mathf.CeilToInt(maxTimeLimit)}";
        enemyManager.ResetEnemies();
        levelManager.ResetLevel();
        playerManager.ResetPlayer();
    }

    public void SetPlayer(GameObject _selectedPlayer)
    {
        selectPlayer = _selectedPlayer;
    }

    public void Restart()
    {
        ResetGame();
        StartGame();
    }

    //�f�o�b�O�p
    public void SetMaxTimeLimit(float _maxTimeLimit)
    {
        maxTimeLimit = _maxTimeLimit;
    }

    public void ResetMaxTimeLimit()
    {
        maxTimeLimit = initmaxTimeLimit;
    }

    public void ToSkinChange()
    {
        state = GameState.SkinChange;
        SelectOnUI(skinChangePanel);
    }
}
