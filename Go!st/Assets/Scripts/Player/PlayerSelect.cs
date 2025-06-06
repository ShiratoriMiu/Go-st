using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerSelect : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] GameObject[] players;
    [SerializeField] float space = 5f;
    [SerializeField] float lerpSpeed = 10f;

    [Header("Swipe Settings")]
    [SerializeField] float swipeThreshold = 100f;
    [SerializeField] float swipeOffsetValue = 0.3f;
    [SerializeField] float skinChangeRotationSpeed = 0.5f;
    [SerializeField] bool debugCanSwipe = false;

    [Header("UI & References")]
    [SerializeField] Text skillEnemyNumText;
    [SerializeField] GameObject selectButton;
    [SerializeField] ColorChanger colorChanger;
    [SerializeField] SkinItemUIManager skinItemUIManager;

    public GameObject selectPlayer { get; private set; }

    private PlayerInputAction action;
    private GameManager gameManager;

    private Vector2 startTouchPoint;
    private bool isSwipe = false;
    private bool isPressing = false;
    private float swipeOffset = 0f;
    private bool isInitialized = false;

    private int count = 0;
    private int lastCount = 0;
    private int nextCount = 0;

    void Awake()
    {
        InitializeInputActions();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        InitializePlayerPositions();
        UpdateSelectionIndices(0);
        selectPlayer = players[0];
        skillEnemyNumText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (debugCanSwipe) action.Enable();
        else action.Disable();

        selectButton.SetActive(players.Length > 1);
    }

    void OnDisable() => action.Disable();

    void Update()
    {
        switch (gameManager.state)
        {
            case GameManager.GameState.Title:
                HandleTitleState();
                break;
            case GameManager.GameState.SkinChange:
                HandleSkinChangeState();
                break;
        }
    }

    private void HandleTitleState()
    {
        if (!isInitialized)
        {
            InitializePlayerPositions();
            isInitialized = true;
            UpdateCharacterPositions(true);
        }
        else if (players.Length > 1)
        {
            UpdateCharacterPositions(false);
        }

        UpdateActiveCharacters();
    }

    private void HandleSkinChangeState()
    {
        action.Enable();
        selectPlayer.transform.position = Vector3.zero;
        UpdateModelRotation();
    }

    private void InitializeInputActions()
    {
        action = new PlayerInputAction();
        action.Title.Swipe.started += OnSwipeStarted;
        action.Title.Swipe.canceled += OnSwipeEnded;
        action.Title.Swipe.performed += OnSwipePerformed;
        action.Title.Press.started += OnPressStarted;
        action.Title.Press.canceled += OnPressEnded;
    }

    private void InitializePlayerPositions()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = new Vector3(-i * space, 0, 0);
            players[i].transform.rotation = Quaternion.identity;
            players[i].GetComponent<Rigidbody>().useGravity = false;
            players[i].GetComponent<PlayerSkill>().SetSkillEnemyNumText(skillEnemyNumText);
        }
    }

    private void UpdateCharacterPositions(bool immediate)
    {
        SetCharacterPosition(0, count, immediate);
        if (count != lastCount) SetCharacterPosition(1, lastCount, immediate);
        if (count != nextCount) SetCharacterPosition(-1, nextCount, immediate);
    }

    private void SetCharacterPosition(int relativeIndex, int playerIndex, bool immediate)
    {
        float targetX = relativeIndex * space + swipeOffset;
        Vector3 targetPos = new Vector3(targetX, 0, 0);

        if (immediate)
            players[playerIndex].transform.position = targetPos;
        else
            players[playerIndex].transform.position = Vector3.Lerp(players[playerIndex].transform.position, targetPos, Time.deltaTime * lerpSpeed);
    }

    private void UpdateModelRotation()
    {
        if (!isSwipe || selectPlayer == null) return;

        Vector2 currentTouch = action.Title.Swipe.ReadValue<Vector2>();
        float rotationAmount = -currentTouch.x * skinChangeRotationSpeed * Time.deltaTime;

        if (Mathf.Abs(rotationAmount) > 0.01f)
            selectPlayer.transform.Rotate(0f, rotationAmount, 0f);
    }

    private void UpdateActiveCharacters()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool active = (i == count || i == lastCount || i == nextCount);
            if (players[i].activeSelf != active)
                players[i].SetActive(active);
        }
    }

    public void NextCount()
    {
        if (gameManager.state == GameManager.GameState.SkinChange || players.Length <= 1) return;

        int newIndex = (count + 1) % players.Length;
        UpdateSelectionIndices(newIndex);

        players[nextCount].transform.position = new Vector3(-2 * space + swipeOffset, 0, 0);
    }

    public void BeforeCount()
    {
        if (gameManager.state == GameManager.GameState.SkinChange || players.Length <= 1) return;

        int newIndex = (count - 1 + players.Length) % players.Length;
        UpdateSelectionIndices(newIndex);

        players[lastCount].transform.position = new Vector3(2 * space + swipeOffset, 0, 0);
    }

    private void UpdateSelectionIndices(int newIndex)
    {
        count = newIndex;
        lastCount = (count - 1 + players.Length) % players.Length;
        nextCount = (count + 1) % players.Length;
        selectPlayer = players[count];
    }

    public void GameStart()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool isSelected = (i == count);
            players[i].SetActive(isSelected);
            players[i].GetComponent<Rigidbody>().useGravity = isSelected;

            if (isSelected) selectPlayer = players[i];
        }

        gameManager.SetPlayer(selectPlayer);
        gameManager.StartGame();
    }

    public void ToSkinChangeSelectedPlayer()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool isSelected = (i == count);
            players[i].SetActive(isSelected);
            if (isSelected) selectPlayer = players[i];
        }

        gameManager.SetPlayer(selectPlayer);
        colorChanger.SetTargetRenderer(selectPlayer.GetComponent<PlayerController>().renderer);
        skinItemUIManager.SetTargetPlayer(selectPlayer.GetComponent<SkinItemTarget>());
    }

    public void ResetSelection()
    {
        UpdateSelectionIndices(0);
        isSwipe = false;
        isPressing = false;
        swipeOffset = 0f;
        InitializePlayerPositions();
    }

    public GameObject[] GetPlayers() => players;

    public void SetAutoAim(bool onAutoAim)
    {
        foreach (var player in players)
        {
            player.GetComponent<PlayerController>().SetAutoAim(onAutoAim);
        }
    }

    // --- Input Event Handlers ---
    private void OnPressStarted(InputAction.CallbackContext context) => isPressing = true;

    private void OnPressEnded(InputAction.CallbackContext context)
    {
        isPressing = false;
        swipeOffset = 0f;
    }

    private void OnSwipeStarted(InputAction.CallbackContext context)
    {
        if (isPressing && !isSwipe)
        {
            startTouchPoint = context.ReadValue<Vector2>();
            isSwipe = true;
        }
    }

    private void OnSwipeEnded(InputAction.CallbackContext context)
    {
        if (!isPressing && isSwipe)
        {
            Vector2 endTouchPoint = context.ReadValue<Vector2>();
            float deltaX = endTouchPoint.x - startTouchPoint.x;

            if (Mathf.Abs(deltaX) > swipeThreshold)
            {
                if (deltaX > 0) NextCount();
                else BeforeCount();
            }

            swipeOffset = 0f;
            isSwipe = false;
        }
    }

    private void OnSwipePerformed(InputAction.CallbackContext context)
    {
        if (isSwipe && isPressing)
        {
            Vector2 currentTouch = context.ReadValue<Vector2>();
            float direction = currentTouch.x - startTouchPoint.x;

            swipeOffset = Mathf.Abs(direction) > 0.01f
                ? -Mathf.Sign(direction) * swipeOffsetValue
                : 0f;
        }
    }
}
