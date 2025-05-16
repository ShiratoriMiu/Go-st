using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{
    [SerializeField] GameObject[] players; // キャラリスト
    [SerializeField] float space = 5f; // キャラ間の距離
    [SerializeField] float swipeThreshold = 100f; // スワイプ判定の閾値（スマホ向け）
    [SerializeField] float lerpSpeed = 10f; // スムーズな移動速度
    [SerializeField] float swipeOffsetValue = 0.3f;
    [SerializeField] Text skillEnemyNumText;
    [SerializeField] bool debugCanSwipe = false;

    PlayerInputAction action;

    int count = 0; // 現在選択中のキャラクターのインデックス

    GameManager gameManager;
    private Vector2 startTouchPoint; // スワイプ開始時のタッチ位置
    private bool isSwipe = false; // スワイプ中かどうか
    private bool isPressing = false; // 画面をタッチしているかどうか
    private float swipeOffset = 0f; // スワイプ中のオフセット量

    int lastCount; // 現在選択中のキャラクターの前のキャラクターのインデックス
    int nextCount; // 現在選択中のキャラクターの次のキャラクターのインデックス

    bool isInitialize = false;

    public GameObject selectPlayer{ get; private set; }

    void Awake()
    {
        // Input Action の初期化
        InitializeInputActions();

        // GameManager の取得
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // プレイヤーの初期位置設定
        InitializePlayerPositions();

        // lastCount, nextCount の初期化
        lastCount = players.Length - 1;
        nextCount = (count + 1) % players.Length;
        selectPlayer = players[0];

        skillEnemyNumText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Input Action の初期化を行う。
    /// </summary>
    private void InitializeInputActions()
    {
        action = new PlayerInputAction();
        action.Title.Swipe.started += OnSwipeStarted;
        action.Title.Swipe.canceled += OnSwipeEnded;
        action.Title.Swipe.performed += OnSwipePerformed;
        action.Title.Press.started += OnPressStarted;
        action.Title.Press.canceled += OnPressEnded;
    }

    /// <summary>
    /// プレイヤーの初期位置を設定する。
    /// </summary>
    private void InitializePlayerPositions()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = new Vector3(-i * space, 0, 0);
            players[i].transform.rotation = new Quaternion(0, 0, 0, 0);
            players[i].GetComponent<Rigidbody>().useGravity = false;
            players[i].GetComponent<PlayerSkill>().SetSkillEnemyNumText(skillEnemyNumText);

            if (i == lastCount)
            {
                players[i].transform.position = new Vector3(1 * space, 0, 0);
            }
            else if (i == nextCount)
            {
                players[i].transform.position = new Vector3(-1 * space, 0, 0);
            }
        }
    }

    private void OnEnable()
    {
        if (debugCanSwipe)
        {
            action.Enable();
        }
        else
        {
            action.Disable();
        }
    }

    private void OnDisable()
    {
        action.Disable();
    }

    void Update()
    {
        // タイトル画面以外では処理を行わない
        if (gameManager.state != GameManager.GameState.Title)
        {
            if (action.Title.enabled)
            {
                isInitialize = false;
                OnDisable();
            }
            return;
        }

        if (!isInitialize)
        {
            InitializePlayerPositions();
            OnEnable();
            isInitialize = true;
        }

        // キャラクターの表示位置を更新
        UpdateCharacterPositions();

        // アクティブなキャラクターの切り替え
        UpdateActiveCharacters();
    }

    /// <summary>
    /// キャラクターの表示位置を更新する。
    /// 現在の count, lastCount, nextCount に基づいて、各キャラクターの位置を計算し、移動させる。
    /// </summary>
    private void UpdateCharacterPositions()
    {
        // lastCount キャラクターの移動
        CharacterPos(1, lastCount);

        // nextCount キャラクターの移動
        CharacterPos(-1, nextCount);

        // count キャラクターの移動
        CharacterPos(0, count);
    }

    void CharacterPos(int _posX, int _playerNum)
    {
        float targetX = _posX * space + swipeOffset;
        players[_playerNum].transform.position = Vector3.Lerp(
            players[_playerNum].transform.position,
            new Vector3(targetX, 0, 0),
            Time.deltaTime * lerpSpeed
        );
    }

    /// <summary>
    /// アクティブなキャラクターを切り替える。
    /// count, lastCount, nextCount のキャラクター以外は非表示にする。
    /// </summary>
    private void UpdateActiveCharacters()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool shouldBeActive = (i == count || i == lastCount || i == nextCount);

            // 現在の状態と必要な状態が異なる場合のみ SetActive を呼ぶ
            if (players[i].activeSelf != shouldBeActive)
            {
                players[i].SetActive(shouldBeActive);
            }
        }
    }

    /// <summary>
    /// 次のキャラクターを選択する。
    /// count, lastCount, nextCount を更新する。
    /// </summary>
    public void NextCount()
    {
        // count の更新
        count = (count + 1) % players.Length;

        // lastCount の更新
        UpdateLastCount();

        // nextCount の更新
        nextCount = (count + 1) % players.Length;

        // 次のキャラクターを画面外から出現させるための初期位置設定
        players[nextCount].transform.position = new Vector3(-2 * space + swipeOffset, 0, 0);
    }

    /// <summary>
    /// 前のキャラクターを選択する。
    /// count, lastCount, nextCount を更新する。
    /// </summary>
    public void BeforeCount()
    {
        // count の更新
        count = (count - 1 + players.Length) % players.Length;

        // lastCount の更新
        UpdateLastCount();

        // 前のキャラクターを画面外から出現させるための初期位置設定
        players[lastCount].transform.position = new Vector3(2 * space + swipeOffset, 0, 0);

        // nextCount の更新
        nextCount = (count + 1) % players.Length;
    }

    /// <summary>
    /// lastCount を更新する。
    /// count に基づいて lastCount を計算する。
    /// </summary>
    private void UpdateLastCount()
    {
        lastCount = count - 1;
        if (lastCount < 0)
        {
            lastCount = players.Length - 1;
        }
    }

    public void GameStart()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].gameObject.SetActive(i == count);
            if(i == count)
            {
                players[i].GetComponent<Rigidbody>().useGravity = true;
                selectPlayer = players[i];
            }
        }

        gameManager.SetPlayer(selectPlayer);
        gameManager.StartGame();
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        isPressing = true;
    }

    private void OnPressEnded(InputAction.CallbackContext context)
    {
        isPressing = false;
        swipeOffset = 0f; // 指を離したらオフセットをリセット
    }

    private void OnSwipeStarted(InputAction.CallbackContext context)
    {
        if (isPressing && !isSwipe) // スワイプが開始されていなければ
        {
            startTouchPoint = context.ReadValue<Vector2>();
            isSwipe = true;
        }
    }

    private void OnSwipeEnded(InputAction.CallbackContext context)
    {
        if (!isPressing && isSwipe) // スワイプが開始されていれば
        {
            Vector2 endTouchPoint = context.ReadValue<Vector2>();
            float deltaX = endTouchPoint.x - startTouchPoint.x;

            if (Mathf.Abs(deltaX) > swipeThreshold)
            {
                if (deltaX > 0)
                {
                    NextCount(); // 左スワイプ
                }
                else
                {
                    BeforeCount(); // 右スワイプ
                }
            }

            swipeOffset = 0f; // スワイプ終了時にオフセットをリセット
            isSwipe = false;
        }
    }

    private void OnSwipePerformed(InputAction.CallbackContext context)
    {
        if (isSwipe && isPressing)
        {
            Vector2 currentTouch = context.ReadValue<Vector2>();
            float direction = currentTouch.x - startTouchPoint.x;

            if (Mathf.Abs(direction) > 0.01f)
            {
                // スワイプ方向に基づいてオフセットを変更
                float swipeDirection = -Mathf.Sign(direction);
                swipeOffset = swipeDirection * swipeOffsetValue;
            }
            else
            {
                swipeOffset = 0f;
            }
        }
    }


    public GameObject[] GetPlayers()
    {
        return players;
    }

    public void ResetSelection()
    {
        count = 0;
        lastCount = players.Length - 1;
        nextCount = (count + 1) % players.Length;
        selectPlayer = players[0];

        isSwipe = false;
        isPressing = false;
        swipeOffset = 0f;

        InitializePlayerPositions();
    }

    public void SetAutoAim(bool _onAutoAim)
    {
        foreach (var player in players)
        {
            player.GetComponent<PlayerController>().SetAutoAim(_onAutoAim);
        }
    }
}