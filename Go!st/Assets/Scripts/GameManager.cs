using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking, SkinChange, Help, Setting, GameSetting, Profile }
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

    [SerializeField] private GameObject rankingTextBase;//���E�����L���O�̃e�L�X�g�𐶐�����e�I�u�W�F�N�g
    [SerializeField] private GameObject rankingTextPrefab;

    [System.Serializable]
    public class UIPanelEntry
    {
        public GameManager.GameState state;
        public GameObject panel;
    }
    [SerializeField] private List<UIPanelEntry> uiPanelEntries;
    private Dictionary<GameState, GameObject> uiPanels;

    private List<Text> rankingTextList = new List<Text>();

    [SerializeField] private float resetSelectPlayerRotateLerpSpeed = 1;

    [SerializeField] private FirebaseController firebaseController;

    CenterToGrayEffect centerToGrayEffect;

    private int finalScore;
    [SerializeField,Header("�\�����郉���L���O��")] private int rankingDisplayNum = 100;//�����L���O�̉��ʂ܂ŕ\�����邩

    //�f�o�b�O�p
    private float initmaxTimeLimit;

    //frebase�r���h���o���Ȃ��̂ňꎞ�I��csv�ɕύX
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

        //�����L���O�p�e�L�X�g�̐���
        for(int i = 0; i < rankingDisplayNum; ++i)
        {
            rankingTextList.Add(Instantiate(rankingTextPrefab,rankingTextBase.transform).GetComponentInChildren<Text>());
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
                EndGame(); // �������Ԃ��s������Q�[���I��
            }
        }
    }

    public void InitGame()
    {
        levelManager.LoadLevelData();
        ResetGame(); // ������܂߂Ė��񏉊���
    }

    void StartGame()
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
        int totalSeconds = Mathf.CeilToInt(time); // �b���𐮐��ɐ؂�グ
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        remainingTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }



    public void AddEnemiesDefeatedNum(int _value)
    {
        enemiesDefeated+= _value;
    }

    public int CalculateScore()
    {
        PlayerController playerController = playerManager.Player.GetComponent<PlayerController>();

        int getHP = playerController.hp;
        if (getHP < 0) getHP = 0;
        Debug.Log("�|�������F" + enemiesDefeated + " / " + "�������ԁF" + Mathf.FloorToInt(survivalTime) + " / " + "�c��HP�F" + getHP);
        //�X�R�A�v�Z
        finalScore = enemiesDefeated + Mathf.FloorToInt(survivalTime) + getHP;
        return finalScore;
    }

    private async void ShowScore()
    {
        int score = CalculateScore();
        await firebaseController.SaveMyScoreAsync(score);
        scoreText.text = $"Score : {score}";
    }


    private async void ShowRanking()
    {
        var topRanks = await firebaseController.GetTopRankingsAsync();
        for (int i = 0; i < topRanks.Count; ++i) {
            rankingTextList[i].text = $"{topRanks[i].rank}��: {topRanks[i].name}: {topRanks[i].score}";
        }
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

        if (playerManager.Player == null) yield break;
        while (Quaternion.Angle(playerManager.Player.transform.rotation, targetRotation) > 0.1f)
        {
            playerManager.Player.transform.rotation = Quaternion.Lerp(
                playerManager.Player.transform.rotation,
                targetRotation,
                Time.deltaTime * resetSelectPlayerRotateLerpSpeed
            );

            yield return null;
        }

        playerManager.Player.transform.rotation = targetRotation;
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

    public void ToTitle()
    {
        ChangeGameState(GameState.Title);
        ResetGame();
        StartCoroutine(ResetSelectPlayerRotate());
    }
    public void ToGame(bool _isReset) 
    {
        if (_isReset)
        {
            playerManager.PlayerGameStart();
            StartGame();
        }
        ChangeGameState(GameState.Game);
    }
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
    public void ToGameSetting() => ChangeGameState(GameState.GameSetting);
    public void ToProfile() => ChangeGameState(GameState.Profile);

    void ChangeGameState(GameState _gameState)
    {
        state = _gameState;
        SwitchUIPanel(_gameState);
    }
}
