using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking, SkinChange, Help, Setting, GameSetting, Profile,Gacha,Shop,StartCountDown, GameUISettingUI}
    public GameState state;

    public PlayerManager playerManager;
    public EnemyManager enemyManager;
    public LevelManager levelManager;

    public int enemiesDefeated = 0;
    public float survivalTime = 0f;

    // 状態変更イベント
    public event System.Action<GameState> OnGameStateChanged;

    [Header("Game Time Settings")]
    [SerializeField] private float maxTimeLimit = 60f; // 最大制限時間（インスペクターで設定）

    [Header("UI")]
    [SerializeField] private Text scoreText;           // スコア表示用
    [SerializeField] private Text remainingTimeText;   // 残り時間表示用

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

    [SerializeField] private MapRandomChanger mapRandomChanger;

    [SerializeField] private CountDownUIController countDownUIController;

    CenterToGrayEffect centerToGrayEffect;

    private int finalScore;

    private int lastTimeInt = -1;    // 前回の整数秒
    private bool isLowTimeEffect = false;
    private Color normalColor = Color.white;
    private Color warningColor = Color.red;

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
        int totalSeconds = Mathf.CeilToInt(time); // 秒数を切り上げ
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        remainingTimeText.text = $"{minutes:00}:{seconds:00}";

        // --- ここから演出処理 ---
        if (totalSeconds <= 5 && totalSeconds >= 0)
        {
            if (totalSeconds != lastTimeInt)  // 秒が変わった瞬間だけ
            {
                PlayTimeWarningAnimation();
                lastTimeInt = totalSeconds;
            }
        }
        else
        {
            lastTimeInt = -1; // リセット
        }
    }

    private void PlayTimeWarningAnimation()
    {
        var rt = remainingTimeText.rectTransform;

        // 拡縮アニメ
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOScale(1.25f, 0.15f)
          .OnComplete(() => rt.DOScale(1f, 0.15f));

        // 点滅（色の切り替え）
        remainingTimeText.DOKill(); // 前回アニメ停止
        remainingTimeText.color = normalColor;
        remainingTimeText.DOColor(warningColor, 0.15f).SetLoops(2, LoopType.Yoyo);
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
        return finalScore;
    }

    private async void ShowScore()
    {
        int score = CalculateScore();
        scoreText.text = $"Score : {score}";
        await firebaseController.SaveMyScoreAsync(score);
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

    public void ToTitle(bool _resetBGM)
    {
        ChangeGameState(GameState.Title);
        ResetGame();
        StartCoroutine(ResetSelectPlayerRotate());
        MapRandomChanger.Instance.OffModelActive();
        if (_resetBGM)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM("HomeBGM", true);
        }
    }
    public void ToGame(bool _isReset) 
    {
        if (_isReset)
        {
            playerManager.PlayerGameStart();
            StartGame();
            //SoundManager.Instance.StopBGM();
            //SoundManager.Instance.PlayBGM("GameBGM", true);
        }
        ChangeGameState(GameState.Game);
    }
    public void ToScore() 
    {
        ChangeGameState(GameState.Score);
        ShowScore();
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySE("ResultSE");
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
    public void ToGacha() => ChangeGameState(GameState.Gacha);
    public void ToShop() => ChangeGameState(GameState.Shop);
    public void ToGameUISettingUI() => ChangeGameState(GameState.GameUISettingUI);
    public void ToStartCountDown() 
    { 
        ChangeGameState(GameState.StartCountDown);
        countDownUIController.StartCountDown(ToGame);
        mapRandomChanger.ActivateRandomMap();
    }

    void ChangeGameState(GameState _gameState)
    {
        state = _gameState;
        OnGameStateChanged?.Invoke(state); // 状態変更を通知
        SwitchUIPanel(_gameState);
    }

    public void OnSkinDecided()
    {
        // 既存のセーブデータをロード（コインや既存アイテムを保持するため）
        PlayerSaveData playerData = SaveManager.Load() ?? new PlayerSaveData();

        SkinItemTarget skinItemTarget = playerManager.Player.GetComponent<SkinItemTarget>();
        if (skinItemTarget != null)
        {
            foreach (var slot in skinItemTarget.ItemSlots)
            {
                if (slot == null) continue;

                // セーブデータの allItems から対応するアイテムを探す
                ItemData savedItem = playerData.allItems.Find(i => i.name == slot.itemName);

                // 見つからなければ新規作成
                if (savedItem == null)
                {
                    savedItem = new ItemData(
                        slot.itemName,
                        slot.itemIcon.name,
                        Color.white,
                        slot.isOwned,
                        slot.isEquipped,
                        slot.canColorChange,
                        slot.currentColorChange,
                        ItemStyle.SkinItem
                    );
                    playerData.allItems.Add(savedItem);
                }

                // 所持・装備状態を更新
                savedItem.isOwned = slot.isOwned;
                savedItem.isEquipped = slot.isEquipped;

                // 色を保存（装備中で Renderer がある場合）
                if (slot.isEquipped && slot.itemObjectRenderers != null && slot.itemObjectRenderers.Length > 0)
                {
                    Color c = slot.itemObjectRenderers[0].material.color;
                    savedItem.color = new float[] { c.r, c.g, c.b, c.a };
                }
            }
        }

        // メイクアップスロットも同様に処理
        foreach (var makeSlot in makeUpManager.MakeUpSlots)
        {
            if (makeSlot == null) continue;

            ItemData savedItem = playerData.allItems.Find(i => i.name == makeSlot.name);
            if (savedItem == null)
            {
                savedItem = new ItemData(
                    makeSlot.name,
                    makeSlot.name,
                    Color.white,
                    makeSlot.isOwned,
                    makeSlot.isEquipped,
                    false,
                    false,
                    ItemStyle.MakeUp
                );
                playerData.allItems.Add(savedItem);
            }

            savedItem.isOwned = makeSlot.isOwned;
            savedItem.isEquipped = makeSlot.isEquipped;
        }

        // カラースロットも処理
        foreach (var colorSlot in colorChanger.SkinSlots)
        {
            if (colorSlot == null) continue;

            ItemData savedItem = playerData.allItems.Find(i => i.name == colorSlot.name);
            if (savedItem == null)
            {
                savedItem = new ItemData(
                    colorSlot.name,
                    colorSlot.icon.name,
                    Color.white,
                    colorSlot.isOwned,
                    false,
                    false,
                    false,
                    ItemStyle.SkinColor
                );
                playerData.allItems.Add(savedItem);
            }

            savedItem.isOwned = colorSlot.isOwned;
            savedItem.isEquipped = false;
        }

        // マテリアル保存
        Renderer playerRenderer = playerManager.Player.GetComponent<PlayerController>().renderer;
        playerData.materials.Clear();
        foreach (var mat in playerRenderer.materials)
        {
            Texture2D tex = mat.mainTexture as Texture2D;
            string texName = tex != null ? tex.name : "";
            playerData.materials.Add(new MaterialData(texName, mat.color));
        }

        // コインだけは既存の値を維持
        playerData.coin = SaveManager.LoadCoin();

        playerData.gachaItems = SaveManager.GachaItems();

        playerData.ownedPlayerIcons = SaveManager.LoadOwnedPlayerIcons();

        playerData.equippedPlayerIcons = SaveManager.LoadEquippedPlayerIcons();

        // 保存
        SaveManager.Save(playerData);

        Debug.Log("スキン決定時にデータ保存完了");
    }

}
