using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerSelect : MonoBehaviour
{
    [SerializeField] GameObject[] players; // �L�������X�g
    [SerializeField] float space = 5f; // �L�����Ԃ̋���
    [SerializeField] float swipeThreshold = 100f; // �X���C�v�����臒l�i�X�}�z�����j
    [SerializeField] float lerpSpeed = 10f; // �X���[�Y�Ȉړ����x
    [SerializeField] float swipeOffsetValue = 0.3f;
    [SerializeField] Text skillEnemyNumText;
    [SerializeField] bool debugCanSwipe = false;
    [SerializeField] ColorChanger colorChanger;
    [SerializeField] SkinItemUIManager skinItemUIManager;
    [SerializeField] GameObject selectButton;

    PlayerInputAction action;

    int count = 0; // ���ݑI�𒆂̃L�����N�^�[�̃C���f�b�N�X

    GameManager gameManager;
    private Vector2 startTouchPoint; // �X���C�v�J�n���̃^�b�`�ʒu
    private bool isSwipe = false; // �X���C�v�����ǂ���
    private bool isPressing = false; // ��ʂ��^�b�`���Ă��邩�ǂ���
    private float swipeOffset = 0f; // �X���C�v���̃I�t�Z�b�g��

    int lastCount; // ���ݑI�𒆂̃L�����N�^�[�̑O�̃L�����N�^�[�̃C���f�b�N�X
    int nextCount; // ���ݑI�𒆂̃L�����N�^�[�̎��̃L�����N�^�[�̃C���f�b�N�X

    bool isInitialize = false;

    public GameObject selectPlayer{ get; private set; }

    void Awake()
    {
        // Input Action �̏�����
        InitializeInputActions();

        // GameManager �̎擾
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // �v���C���[�̏����ʒu�ݒ�
        InitializePlayerPositions();

        // lastCount, nextCount �̏�����
        lastCount = players.Length - 1;
        nextCount = (count + 1) % players.Length;
        selectPlayer = players[0];

        skillEnemyNumText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Input Action �̏��������s���B
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
    /// �v���C���[�̏����ʒu��ݒ肷��B
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
        if (debugCanSwipe && players.Length > 1)
        {
            action.Enable();
            selectButton.SetActive(true);
        }
        else
        {
            action.Disable();
            selectButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        action.Disable();
    }

    void Update()
    {
        // �^�C�g����ʈȊO�ł͏������s��Ȃ�
        if (gameManager.state != GameManager.GameState.Title)
        {
            if (action.Title.enabled)
            {
                isInitialize = false;
                OnDisable();
            }

            if (gameManager.state == GameManager.GameState.SkinChange)
            {
                if (players.Length > 1) UpdateCharacterPositions(false); // SkinChange����Lerp�Ŋ��炩�Ɉړ�
            }

            return;
        }

        if (!isInitialize)
        {
            InitializePlayerPositions();
            isInitialize = true;
            UpdateCharacterPositions(true); // ���������ő����z�u�I
        }
        else
        {
           UpdateCharacterPositions(false); // �ʏ��Lerp����
        }

        UpdateActiveCharacters();
    }


    /// <summary>
    /// �L�����N�^�[�̕\���ʒu���X�V����B
    /// ���݂� count, lastCount, nextCount �Ɋ�Â��āA�e�L�����N�^�[�̈ʒu���v�Z���A�ړ�������B
    /// </summary>
    private void UpdateCharacterPositions(bool immediateMove)
    {
        if(players.Length > 1)
        {
            CharacterPos(1, lastCount, immediateMove);
            CharacterPos(-1, nextCount, immediateMove);
        }
        CharacterPos(0, count, immediateMove);
    }


    void CharacterPos(int _posX, int _playerNum, bool immediateMove)
    {
        float targetX = _posX * space + swipeOffset;
        Vector3 targetPos = new Vector3(targetX, 0, 0);

        if (immediateMove)
        {
            players[_playerNum].transform.position = targetPos;
        }
        else
        {
            players[_playerNum].transform.position = Vector3.Lerp(
                players[_playerNum].transform.position,
                targetPos,
                Time.deltaTime * lerpSpeed
            );
        }
    }


    /// <summary>
    /// �A�N�e�B�u�ȃL�����N�^�[��؂�ւ���B
    /// count, lastCount, nextCount �̃L�����N�^�[�ȊO�͔�\���ɂ���B
    /// </summary>
    private void UpdateActiveCharacters()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool shouldBeActive = (i == count || i == lastCount || i == nextCount);

            // ���݂̏�ԂƕK�v�ȏ�Ԃ��قȂ�ꍇ�̂� SetActive ���Ă�
            if (players[i].activeSelf != shouldBeActive)
            {
                players[i].SetActive(shouldBeActive);
            }
        }
    }

    /// <summary>
    /// ���̃L�����N�^�[��I������B
    /// count, lastCount, nextCount ���X�V����B
    /// </summary>
    public void NextCount()
    {
        // count �̍X�V
        count = (count + 1) % players.Length;

        // lastCount �̍X�V
        UpdateLastCount();

        // nextCount �̍X�V
        nextCount = (count + 1) % players.Length;

        // ���̃L�����N�^�[����ʊO����o�������邽�߂̏����ʒu�ݒ�
        players[nextCount].transform.position = new Vector3(-2 * space + swipeOffset, 0, 0);
    }

    /// <summary>
    /// �O�̃L�����N�^�[��I������B
    /// count, lastCount, nextCount ���X�V����B
    /// </summary>
    public void BeforeCount()
    {
        // count �̍X�V
        count = (count - 1 + players.Length) % players.Length;

        // lastCount �̍X�V
        UpdateLastCount();

        // �O�̃L�����N�^�[����ʊO����o�������邽�߂̏����ʒu�ݒ�
        players[lastCount].transform.position = new Vector3(2 * space + swipeOffset, 0, 0);

        // nextCount �̍X�V
        nextCount = (count + 1) % players.Length;
    }

    /// <summary>
    /// lastCount ���X�V����B
    /// count �Ɋ�Â��� lastCount ���v�Z����B
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

    public void ToSkinChangeSelectedPlayer()
    {
        for (int i = 0; i < players.Length; i++)
        {
            bool isActive = (i == count);
            players[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                selectPlayer = players[i];
            }
        }
        gameManager.SetPlayer(selectPlayer);
        colorChanger.SetTargetRenderer(selectPlayer.GetComponent<PlayerController>().renderer);
        skinItemUIManager.SetTargetPlayer(selectPlayer.GetComponent<SkinItemTarget>());
    }


    private void OnPressStarted(InputAction.CallbackContext context)
    {
        isPressing = true;
    }

    private void OnPressEnded(InputAction.CallbackContext context)
    {
        isPressing = false;
        swipeOffset = 0f; // �w�𗣂�����I�t�Z�b�g�����Z�b�g
    }

    private void OnSwipeStarted(InputAction.CallbackContext context)
    {
        if (isPressing && !isSwipe) // �X���C�v���J�n����Ă��Ȃ����
        {
            startTouchPoint = context.ReadValue<Vector2>();
            isSwipe = true;
        }
    }

    private void OnSwipeEnded(InputAction.CallbackContext context)
    {
        if (!isPressing && isSwipe) // �X���C�v���J�n����Ă����
        {
            Vector2 endTouchPoint = context.ReadValue<Vector2>();
            float deltaX = endTouchPoint.x - startTouchPoint.x;

            if (Mathf.Abs(deltaX) > swipeThreshold)
            {
                if (deltaX > 0)
                {
                    NextCount(); // ���X���C�v
                }
                else
                {
                    BeforeCount(); // �E�X���C�v
                }
            }

            swipeOffset = 0f; // �X���C�v�I�����ɃI�t�Z�b�g�����Z�b�g
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
                // �X���C�v�����Ɋ�Â��ăI�t�Z�b�g��ύX
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