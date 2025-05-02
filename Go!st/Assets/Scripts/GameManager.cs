using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking }
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

    [SerializeField] private ScoreManager scoreManager;

    CenterToGrayEffect centerToGrayEffect;

    public GameObject selectPlayer { get; private set; }

    private int finalScore;

    private void Start()
    {
        InitGame();
        SelectOnUI(titlePanel);
        centerToGrayEffect = Camera.main.GetComponent<CenterToGrayEffect>();
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
        await scoreManager.WriteScoreDataAsync(score);
        scoreText.text = $"Score : {score}";
    }


    public void ShowRanking()
    {
        state = GameState.Ranking;
        SelectOnUI(rankingPanel);

        List<int> scores = scoreManager.LoadScoreData();
        scores.Sort((a, b) => b.CompareTo(a)); // �~���\�[�g

        int count = Mathf.Min(scores.Count, 5);

        string rankingResult = "";
        for (int i = 0; i < count; i++)
        {
            rankingResult += $"{i + 1}��: {scores[i]}\n";
        }

        rankingText.text = rankingResult;
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
}
