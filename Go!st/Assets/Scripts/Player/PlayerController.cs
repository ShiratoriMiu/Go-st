using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //�ړ�
    [SerializeField] float skillAddSpeed = 1.5f;//�K�E�Z���ɃX�s�[�h���グ���

    [SerializeField,Header("�Q�[�W�̍���")] private float maxSkillChargeImageHeight = 400f;

    [SerializeField] GameObject stickController;//���z�X�e�B�b�N

    [SerializeField] LevelUpText levelUpText;

    [SerializeField] Renderer rendererInit;
    [SerializeField] private ParticleSystem levelUpEffect;
    [SerializeField] private RectTransform skillGaugeImage;
    [SerializeField] private RectTransform levelUpGaugeImageRect;
    private Image levelUpGaugeImage;
    [SerializeField] private GameManager gameManager;

    public Renderer renderer { get; private set; }
    public bool canSkill { get; private set; }

    Rigidbody rb;
    RectTransform stickControllerRect;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 stickControllerInitPos;

    private Vector3 velocity;

    private bool isInteracting = false;//���͒��t���O
    private bool isAttack = false;//�U�����t���O
    private bool isDamage = false;

    private bool onAutoAim = false;
    private bool canControl = false; // �v���C���[����\���ǂ���

    private float touchTime = 0;

    //���t�@�N�^�����O��
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] PlayerSkill playerSkill;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerMove playerMove;

    //�����ւ����̃L������]
    [SerializeField] private float rotationSpeed = 0;
    private bool skinChangeIsInteracting = false;
    private Vector2 lastTouchPos;
    private Vector2 currentTouchPos;

    private float rotationVelocity = 0f;   // ���݂̉�]���x
    [SerializeField] private float rotationDamping = 5f; // �����̑���


    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        renderer = rendererInit;
        playerHealth.Init();
        stickControllerRect = stickController.GetComponent<RectTransform>();
        stickControllerInitPos = stickControllerRect.anchoredPosition;

        playerSkill.InitializeSkillDependencies(stickController, () => canSkill, SetCanSkill);

        levelUpGaugeImage = levelUpGaugeImageRect.GetComponent<Image>();
    }

    private void Start()
    {
        // �C�x���g�o�^
        InputManager.Instance.OnTouchStart += OnTouchStart;
        InputManager.Instance.OnTouchMove += OnTouchMove;
        InputManager.Instance.OnTouchEnd += OnTouchEnd;
    }

    private void Update()
    {
        if (gameManager.state == GameManager.GameState.SkinChange)
        {
            if (skinChangeIsInteracting)
            {
                HandleSkinChangeRotation();
            }
            else
            {
                // �����ŉ�]
                if (Mathf.Abs(rotationVelocity) > 0.001f)
                {
                    transform.Rotate(0f, -rotationVelocity * Time.deltaTime, 0f, Space.World);

                    // ���� (exp ����)
                    rotationVelocity = Mathf.Lerp(rotationVelocity, 0f, rotationDamping * Time.deltaTime);
                }
            }
            return;
        }

        if (gameManager.state != GameManager.GameState.Game)
        {
            return;
        }
        else
        {
            if (!canControl)
            {
                canControl = true;
                Init();
            }
        }

        if (isInteracting)
        {
            touchTime += Time.deltaTime;
        }
        else
        {
            touchTime = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameManager.state != GameManager.GameState.Game) return;

        if (isInteracting)
        {
            //�ړ�
            if(!playerSkill.GetIsSkill() && !playerSkill.isOneHand || playerSkill.isOneHand) playerMove.Move();
            //����
            playerMove.Inertia(isDamage);
            //�X�e�B�b�N
            Stick();
            //�ړ�����������
            playerMove.LookMoveDirection();
        }

        //skill
        playerSkill.SkillUpdate(isInteracting, currentPosition, Attack, startPosition);
        UpdateSkillChargeImagePosition();

        if (!levelUpEffect.IsAlive())
        {
            levelUpEffect.Stop();
        }
    }

    void Attack()
    {
        //attack
        if (!isAttack)
        {
            isAttack = true;
            playerAttack.Attack();

            Invoke("StopAttack", playerAttack.GetAttackCooldownTime());
        }
    }

    private void UpdateSkillChargeImagePosition()
    {
        float y = playerSkill.coolTime * maxSkillChargeImageHeight;
        skillGaugeImage.anchoredPosition = new Vector2(skillGaugeImage.anchoredPosition.x, y);
    }

    public void UpdateLevelUpImage(float _fillAmount)
    {
        levelUpGaugeImage.fillAmount = _fillAmount;
    }

    private void HandleSkinChangeRotation()
    {
        if (lastTouchPos != Vector2.zero)
        {
            float deltaX = currentTouchPos.x - lastTouchPos.x;
            rotationVelocity = deltaX * rotationSpeed; // velocity�Ƃ��ĕێ�
            transform.Rotate(0f, -rotationVelocity * Time.deltaTime, 0f, Space.World);
        }

        lastTouchPos = currentTouchPos;
    }


    private void OnDisable()
    {
        // Input�A�N�V�����𖳌���
        OnDestroy();
    }

    private void OnDestroy()
    {
        // �C�x���g����
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTouchStart -= OnTouchStart;
            InputManager.Instance.OnTouchMove -= OnTouchMove;
            InputManager.Instance.OnTouchEnd -= OnTouchEnd;
        }
    }

    // �^�b�`�܂��̓}�E�X�̊J�n�ʒu
    private void OnTouchStart(Vector2 position)
    {
        if (gameManager.state == GameManager.GameState.SkinChange)
        {
            skinChangeIsInteracting = true;
            lastTouchPos = position;
            currentTouchPos = position;
            rotationVelocity = 0f;
        }

        if (gameManager.state != GameManager.GameState.Game || !canControl) return;

        startPosition = position;
        currentPosition = startPosition;
        isInteracting = true;

        if (playerSkill.isOneHand)
        {
            // �X�e�B�b�N�w�i���w��ʒu��
            stickController.transform.position = startPosition;

            // �X�e�B�b�N�����m�u�i�q�I�u�W�F�N�g�j�𓯂��ʒu��
            stickController.transform.GetChild(0).position = startPosition;
        }
        else
        {
            if (!playerSkill.GetIsSkill())
            {
                // �X�e�B�b�N�w�i���w��ʒu��
                stickController.transform.position = startPosition;

                // �X�e�B�b�N�����m�u�i�q�I�u�W�F�N�g�j�𓯂��ʒu��
                stickController.transform.GetChild(0).position = startPosition;
            }
        }
    }

    // �^�b�`�܂��̓}�E�X�̌��݈ʒu
    private void OnTouchMove(Vector2 position)
    {
        if (gameManager.state == GameManager.GameState.SkinChange && skinChangeIsInteracting)
        {
            currentTouchPos = position; // �ʒu�����L�^
            return;
        }

        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        currentPosition = position;
        playerMove.UpdateMoveDir(currentPosition, startPosition);
    }

    // �^�b�`�܂��̓}�E�X����̏I��
    private void OnTouchEnd()
    {
        if (gameManager.state == GameManager.GameState.SkinChange)
        {
            skinChangeIsInteracting = false;
            lastTouchPos = Vector2.zero;
            currentTouchPos = Vector2.zero;
        }

        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        if (playerSkill.GetIsSkill())
        {
            playerSkill.StopSkill(startPosition, InitializeStick); // �R�[���o�b�N�œn��
        }
        else
        {
            InitializeStick(); // �X�L�����łȂ���Α��߂�
        }

        isInteracting = false;
        playerMove.Init();
    }


    private void InitializeStick()
    {
        stickControllerRect.anchoredPosition = stickControllerInitPos;
        stickController.transform.GetChild(0).gameObject.transform.position = stickControllerRect.transform.position;
    }


    void Stick()
    {
        //�X�e�B�b�N�̈ʒu���^�b�`�����ʒu���痣�ꂷ���Ȃ��悤��
        Vector2 stickPos = Vector2.ClampMagnitude(currentPosition - startPosition, 50);
        stickController.transform.GetChild(0).gameObject.transform.position = startPosition + stickPos;
    }

    void StopAttack()
    {
        isAttack = false;    //�U�����t���O���낷
    }

    //�X�L�����t���O�擾
    public bool GetIsSkill() { return playerSkill.GetIsSkill(); }


    public void Init()
    {
        playerSkill.Init(startPosition);
        canSkill = false;
        isInteracting = false;
        playerMove.Init();
        currentPosition = Vector2.zero;
        transform.position = Vector2.zero;
        InitializeStick();
        playerHealth.Init();
        playerSkill.ResetSkillCoolTime();
        UpdateSkillChargeImagePosition();
    }

    public void Damage(int _num)
    {
        playerHealth.Damage(_num);
    }

    public void ApplyBuff(BuffSO buff)
    {
        if(buff.buffType == BuffType.Heal)
        {
            playerHealth.ApplyBuff(buff);
        }
        else if(buff.buffType == BuffType.SpeedBoost)
        {
            // ���x�A�b�v
            if (buff.speedMultiplier != 1f)
                StartCoroutine(SpeedBuffCoroutine(buff.speedMultiplier, buff.duration));
        }
        else if(buff.buffType == BuffType.SkillCoolTime)
        {
            /*
            if (skillCooldownTime < maxSkillCooldownTime)
            {
                skillCooldownTime = Mathf.Min(skillCooldownTime + buff.skillCoolTimeAdd, maxSkillCooldownTime);
                skillChargeImage.fillAmount = skillCooldownTime / maxSkillCooldownTime;
            }
            */
        }
    }

    private IEnumerator SpeedBuffCoroutine(float multiplier, float duration)
    {
        playerMove.AddSpeed(multiplier);
        Debug.Log($"���x�A�b�v�F{multiplier}�{");

        yield return new WaitForSeconds(duration);

        playerMove.RemoveSpeed(multiplier);
        Debug.Log("���x���ɖ߂����I");
    }

    public void LevelUpText()
    {
        levelUpText.PlayAnimation();
        levelUpEffect.Play();
    }

    public void SetAttackParameters(int _bulletNum, float _attackSpeed, float _attackCooldownTime)
    {
        playerAttack.SetBulletNum(_bulletNum);
        playerAttack.SetAttackSpeed(_attackSpeed);
        playerAttack.SetAttackCooldownTime(_attackCooldownTime);
    }

    public void SetAutoAim(bool _onAutoAim)
    {
        onAutoAim = _onAutoAim;
    }

    public void SetSkillGaugeImage(RectTransform _skillGaugeImage, RectTransform _levelUpGaugeImageRect)
    {
        skillGaugeImage = _skillGaugeImage;
        levelUpGaugeImageRect = _levelUpGaugeImageRect;
        levelUpGaugeImage = levelUpGaugeImageRect.GetComponent<Image>();
    }

    public void SwitchStickPos()
    {
        stickControllerInitPos.x *= -1;
        InitializeStick();
    }

    public int GetHP() => playerHealth.GetHp();

    void SetCanSkill(bool _canSkill) => canSkill = _canSkill;
}