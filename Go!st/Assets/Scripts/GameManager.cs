using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking, SkinChange, Help, Setting }
    public GameState state;

    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public LevelManager levelManager;

    public int enemiesDefeated = 0;
    public float survivalTime = 0f;

    [Header("Game Time Settings")]
    [SerializeField] private float maxTimeLimit = 60f; // 最大制限時間（インスペクターで設定）

    [Header("UI")]
    [SerializeField] private Text scoreText;           // スコア表示用
    [SerializeField] private Text remainingTimeText;   // 残り時間表示用
    [SerializeField] private Text rankingText;         //ランキング表示用


    [System.Serializable]
    public class UIPanelEntry
    {
        public GameManager.GameState state;
        public GameObject panel;
    }
    [SerializeField] private List<UIPanelEntry> uiPanelEntries;
    private Dictionary<GameState, GameObject> uiPanels;

    //[SerializeField] private GameObject gamePanel;
    //[SerializeField] private GameObject titlePanel;
    //[SerializeField] private GameObject scorePanel;
    //[SerializeField] private GameObject rankingPanel;
    //[SerializeField] private GameObject skinChangePanel;
    //[SerializeField] private GameObject helpPanel;

    [SerializeField] private float resetSelectPlayerRotateLerpSpeed = 1;

    [SerializeField] private FirebaseController firebaseController;

    CenterToGrayEffect centerToGrayEffect;

    public GameObject selectPlayer { get; private set; }

    private int finalScore;

    //デバッグ用
    private float initmaxTimeLimit;

    //frebaseビルドが出来ないので一時的にcsvに変更
    //[SerializeField] ScoreManager scoreManager;

    private void Awake()
    {
        uiPanels = new Dictionary<GameManager.GameState, GameObject>();
        foreach (var entry in uiPanelEntries)
        {
            if (!uiPanels.ContainsKey(entry.state))
            {
                uiPanels.Add(entry.state, entry.panel);
            }
            else
            {
                Debug.LogWarning($"Duplicate GameState key found: {entry.state}");
            }
        }
    }


    private void Start()
    {
#if UNITY_STANDALONE_WIN
        PlayerPrefs.DeleteAll();
        Screen.SetResolution(540, 960, false);
#endif
        InitGame();
        SwitchUIPanel(GameState.Title);
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
                EndGame(); // 制限時間が尽きたらゲーム終了
            }
        }
    }

    public void InitGame()
    {
        levelManager.LoadLevelData();
        ResetGame(); // 初回も含めて毎回初期化
    }

    public void StartGame()
    {
        state = GameState.Game;
        SwitchUIPanel(GameState.Game);
        levelManager.InitializeLevel();
        enemyManager.SetEnemyTypesByLevelData(levelManager.levelDataDict[1]);
        playerManager.InitializePlayer(Vector3.zero);
        enemyManager.StartSpawning();
    }

    private void SwitchUIPanel(GameState state)
    {
        foreach (var panel in uiPanels.Values)
        {
            panel.SetActive(false);
        }

        if (uiPanels.TryGetValue(state, out var panelToActivate))
        {
            panelToActivate.SetActive(true);
        }
    }

    private void UpdateTimeUI(float time)
    {
        // 時間を分と秒に変換
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.CeilToInt(time % 60);

        // フォーマットして表示（2桁表示）
        remainingTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public void AddEnemiesDefeatedNum(int _value)
    {
        enemiesDefeated+= _value;
    }

    public int CalculateScore()
    {
        PlayerController playerController = selectPlayer.GetComponent<PlayerController>();

        int getHP = playerController.hp;
        if (getHP < 0) getHP = 0;
        Debug.Log("倒した数：" + enemiesDefeated + " / " + "生存時間：" + Mathf.FloorToInt(survivalTime) + " / " + "残りHP：" + getHP);
        //スコア計算
        finalScore = enemiesDefeated + Mathf.FloorToInt(survivalTime) + getHP;
        return finalScore;
    }

    private async void ShowScore()
    {
        int score = CalculateScore();
        //await scoreManager.WriteScoreDataAsync(score);
        await firebaseController.SaveMyScoreAsync(score);
        scoreText.text = $"Score : {score}";
    }


    private async void ShowRanking()
    {
        var topRanks = await firebaseController.GetTopRankingsAsync();
        string rankText = "";
        for (int i = 0; i < topRanks.Count; ++i) {
            if (i == 0) rankText += $"{topRanks[i].name}: {topRanks[i].score}";
            else rankText += $"\n{topRanks[i].name}: {topRanks[i].score}";
        }
        rankingText.text = rankText;
    }

    public void EndGame()
    {
        if (state != GameState.Game) return;

        ToScore();
        enemyManager.StopSpawning();
        remainingTimeText.text = "00 : 00";
        centerToGrayEffect.ResetGrey();
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

    //デバッグ用
    public void SetMaxTimeLimit(float _maxTimeLimit)
    {
        maxTimeLimit = _maxTimeLimit;
    }

    public void ResetMaxTimeLimit()
    {
        maxTimeLimit = initmaxTimeLimit;
    }

    public void ToTitle()
    {
        ChangeGameState(GameState.Title);
        ResetGame();
        StartCoroutine(ResetSelectPlayerRotate());
    }
    public void ToGame() => ChangeGameState(GameState.Game);
    public void ToScore() 
    {
        ChangeGameState(GameState.Score);
        ShowScore();
    }
    public void ToRanking()
    {
        ShowRanking();
        ChangeGameState(GameState.Ranking);
    }
    public void ToSkinChange() => ChangeGameState(GameState.SkinChange);
    public void ToHelp() => ChangeGameState(GameState.Help);
    public void ToSetting() => ChangeGameState(GameState.Setting);

    void ChangeGameState(GameState _gameState)
    {
        state = _gameState;
        SwitchUIPanel(_gameState);
    }
}
