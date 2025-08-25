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
    [SerializeField] private float maxTimeLimit = 60f; // 最大制限時間（インスペクターで設定）

    [Header("UI")]
    [SerializeField] private Text scoreText;           // スコア表示用
    [SerializeField] private Text remainingTimeText;   // 残り時間表示用
    [SerializeField] private CoinUI coinUI;

    [System.Serializable]
    public class UIPanelEntry
    {
        public GameManager.GameState state;
        public GameObject panel;
    }
    [SerializeField] private List<UIPanelEntry> uiPanelEntries;
    private Dictionary<GameState, GameObject> uiPanels;



    [SerializeField] private float resetSelectPlayerRotateLerpSpeed = 1;

    [SerializeField] private FirebaseController firebaseController;

    [SerializeField] private RankingManager rankingManager;

    [SerializeField] private MakeUpManager makeUpManager;

    [SerializeField] private ColorChanger colorChanger;

    CenterToGrayEffect centerToGrayEffect;

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
        playerManager.ToSkinChangeSelectedPlayer();
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
        int totalSeconds = Mathf.CeilToInt(time); // 秒数を整数に切り上げ
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

        int getHP = playerController.GetHP();
        if (getHP < 0) getHP = 0;
        Debug.Log("倒した数：" + enemiesDefeated + " / " + "生存時間：" + Mathf.FloorToInt(survivalTime) + " / " + "残りHP：" + getHP);
        //スコア計算
        finalScore = enemiesDefeated + Mathf.FloorToInt(survivalTime) + getHP;
        //コイン枚数加算s
        int coinNum = SaveManager.LoadCoin();
        coinNum += enemiesDefeated;
        SaveManager.SaveCoin(coinNum);
        coinUI.UpdateCoinUI();
        return finalScore;
    }

    private async void ShowScore()
    {
        int score = CalculateScore();
        await firebaseController.SaveMyScoreAsync(score);
        scoreText.text = $"Score : {score}";
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
        rankingManager.OnRankingButtonClicked();
        ChangeGameState(GameState.Ranking);
    }
    public void ToSkinChange() {
        playerManager.ToSkinChangeSelectedPlayer();
        ChangeGameState(GameState.SkinChange); 
    }
    public void ToHelp() => ChangeGameState(GameState.Help);
    public void ToSetting() => ChangeGameState(GameState.Setting);
    public void ToGameSetting() => ChangeGameState(GameState.GameSetting);
    public void ToProfile() => ChangeGameState(GameState.Profile);

    void ChangeGameState(GameState _gameState)
    {
        state = _gameState;
        SwitchUIPanel(_gameState);
    }

    public void OnSkinDecided()
    {
        PlayerSaveData playerData = new PlayerSaveData();

        List<string> currentOwnedSkinNames = new List<string>();

        SkinItemTarget skinItemTarget = playerManager.Player.GetComponent<SkinItemTarget>();
        if (skinItemTarget != null)
        {
            foreach (var skinItems in skinItemTarget.ItemSlots)
            {
                if (skinItems.isOwned)
                {
                    currentOwnedSkinNames.Add(skinItems.itemName);
                }

                if (skinItems.isEquipped)
                {
                    // 装備中スキンは色情報も保存
                    if (skinItems.itemObjectRenderers != null && skinItems.itemObjectRenderers.Length > 0)
                    {
                        // 代表として最初のRendererを使う
                        Material mat = skinItems.itemObjectRenderers[0].material;
                        Color c = mat.color;
                        playerData.equippedSkins.Add(new EquippedSkinData(skinItems.itemName, c));
                    }
                    else
                    {
                        // 色が取れない場合は白で保存
                        playerData.equippedSkins.Add(new EquippedSkinData(skinItems.itemName, Color.white));
                    }
                }
            }
        }

        foreach (var makeSlot in makeUpManager.MakeUpSlots)
        {
            if (makeSlot.isEquipped)
            {
                playerData.equippedMakes.Add(makeSlot.name);
            }

            if (makeSlot.isOwned)
            {
                currentOwnedSkinNames.Add(makeSlot.name);
            }
        }

        foreach(var colorSlot in colorChanger.SkinSlots)
        {
            if (colorSlot.isOwned)
            {
                currentOwnedSkinNames.Add(colorSlot.name);
            }
        }

        // 所持スキン
        playerData.ownedSkins = currentOwnedSkinNames;

        // マテリアル
        Renderer playerRenderer = playerManager.Player.GetComponent<PlayerController>().renderer;
        playerData.materials.Clear();
        foreach (var mat in playerRenderer.materials)
        {
            Texture2D tex = mat.mainTexture as Texture2D;
            string texName = tex != null ? tex.name : "";
            playerData.materials.Add(new MaterialData(texName, mat.color));
        }

        // 保存
        SaveManager.Save(playerData);

        Debug.Log("スキン決定時にデータ保存完了");
    }
}
