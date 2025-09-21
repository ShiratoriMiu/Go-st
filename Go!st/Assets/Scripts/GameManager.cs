using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Game, Score, Ranking, SkinChange, Help, Setting, GameSetting, Profile,Gacha,Shop }
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

    [SerializeField] private MapRandomChanger mapRandomChanger;

    CenterToGrayEffect centerToGrayEffect;

    private int finalScore;
    
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

        int getHP = playerController.GetHP();
        if (getHP < 0) getHP = 0;
        Debug.Log("�|�������F" + enemiesDefeated + " / " + "�������ԁF" + Mathf.FloorToInt(survivalTime) + " / " + "�c��HP�F" + getHP);
        //�X�R�A�v�Z
        finalScore = enemiesDefeated + Mathf.FloorToInt(survivalTime) + getHP;
        //�R�C���������Zs
        int coinNum = SaveManager.LoadCoin();
        coinNum += enemiesDefeated;
        SaveManager.SaveCoin(coinNum);
        coinUI.UpdateCoinUI();
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

    //�f�o�b�O�p
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
            mapRandomChanger.ActivateRandomMap();
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

    void ChangeGameState(GameState _gameState)
    {
        state = _gameState;
        SwitchUIPanel(_gameState);
    }

    public void OnSkinDecided()
    {
        // �����̃Z�[�u�f�[�^�����[�h�i�R�C��������A�C�e����ێ����邽�߁j
        PlayerSaveData playerData = SaveManager.Load() ?? new PlayerSaveData();

        SkinItemTarget skinItemTarget = playerManager.Player.GetComponent<SkinItemTarget>();
        if (skinItemTarget != null)
        {
            foreach (var slot in skinItemTarget.ItemSlots)
            {
                if (slot == null) continue;

                // �Z�[�u�f�[�^�� allItems ����Ή�����A�C�e����T��
                ItemData savedItem = playerData.allItems.Find(i => i.name == slot.itemName);

                // ������Ȃ���ΐV�K�쐬
                if (savedItem == null)
                {
                    savedItem = new ItemData(
                        slot.itemName,
                        slot.itemIcon.name,
                        Color.white,
                        slot.isOwned,
                        slot.isEquipped,
                        slot.canColorChange,
                        slot.currentColorChange
                    );
                    playerData.allItems.Add(savedItem);
                }

                // �����E������Ԃ��X�V
                savedItem.isOwned = slot.isOwned;
                savedItem.isEquipped = slot.isEquipped;

                // �F��ۑ��i�������� Renderer ������ꍇ�j
                if (slot.isEquipped && slot.itemObjectRenderers != null && slot.itemObjectRenderers.Length > 0)
                {
                    Color c = slot.itemObjectRenderers[0].material.color;
                    savedItem.color = new float[] { c.r, c.g, c.b, c.a };
                }
            }
        }

        // ���C�N�A�b�v�X���b�g�����l�ɏ���
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
                    false
                );
                playerData.allItems.Add(savedItem);
            }

            savedItem.isOwned = makeSlot.isOwned;
            savedItem.isEquipped = makeSlot.isEquipped;
        }

        // �J���[�X���b�g������
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
                    false
                );
                playerData.allItems.Add(savedItem);
            }

            savedItem.isOwned = colorSlot.isOwned;
            savedItem.isEquipped = false;
        }

        // �}�e���A���ۑ�
        Renderer playerRenderer = playerManager.Player.GetComponent<PlayerController>().renderer;
        playerData.materials.Clear();
        foreach (var mat in playerRenderer.materials)
        {
            Texture2D tex = mat.mainTexture as Texture2D;
            string texName = tex != null ? tex.name : "";
            playerData.materials.Add(new MaterialData(texName, mat.color));
        }

        // �R�C�������͊����̒l���ێ�
        playerData.coin = SaveManager.LoadCoin();

        playerData.gachaItems = SaveManager.GachaItems();

        playerData.ownedPlayerIcons = SaveManager.LoadOwnedPlayerIcons();

        playerData.equippedPlayerIcons = SaveManager.LoadEquippedPlayerIcons();

        // �ۑ�
        SaveManager.Save(playerData);

        Debug.Log("�X�L�����莞�Ƀf�[�^�ۑ�����");
    }

}
