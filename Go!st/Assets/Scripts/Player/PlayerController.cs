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
    [SerializeField] float maxSkillTime = 7f; // �K�E�Z�̍ő�p������
    [SerializeField,Header("�Q�[�W�̍���")] private float maxSkillChargeImageHeight = 400f;

    [SerializeField] GameObject stickController;//���z�X�e�B�b�N
    [SerializeField] GameObject skillChargeEffect; //�X�L�������\�G�t�F�N�g 
    [SerializeField] LevelUpText levelUpText;

    //��ʓ��Ɉړ��͈͂𐧌�
    [SerializeField] LayerMask groundLayer; // �n�ʂ̃��C���[

    [SerializeField, Range(0f, 1f)] float leftOffset = 0.1f;   // ���[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float rightOffset = 0.9f;  // �E�[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float topOffset = 0.9f;    // ��[�̃I�t�Z�b�g
    [SerializeField, Range(0f, 1f)] float bottomOffset = 0.1f; // ���[�̃I�t�Z�b�g

    [SerializeField] CenterToGrayEffect centerToGrayEffect;
    [SerializeField] Renderer rendererInit;
    [SerializeField] PlayerSkillAnim playerSkillAnim;
    [SerializeField] private ParticleSystem levelUpEffect;
    [SerializeField] private Button skillButton;
    [SerializeField] private RectTransform skillGaugeImage;
    [SerializeField] private GameManager gameManager;

    public Renderer renderer { get; private set; }
    public bool canSkill { get; private set; }

    Rigidbody rb;
    PlayerSkill skill;
    Camera mainCamera; // �g�p����J����
    RectTransform stickControllerRect;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 stickControllerInitPos;

    private Vector3 velocity;
    private Vector3[] corners = new Vector3[4]; // �l�p�`�̒��_

    private bool isInteracting = false;//���͒��t���O
    private bool isAttack = false;//�U�����t���O
    private bool isDamage = false;
    private bool isSkill = false;//�K�E�Z�t���O
    private bool isSkillEndEffect = false;//�K�E�Z�t���O
    private bool onAutoAim = false;
    private bool canSkillLine = false;//�X�L���̐����������Ԃ�
    private bool canControl = false; // �v���C���[����\���ǂ���

    private float touchTime = 0;

    //�X�L���������͓G�Ɠ�����Ȃ�����
    private int playerLayer;
    private int enemyLayer;

    //���t�@�N�^�����O��
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] PlayerSkill playerSkill;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerMove playerMove;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        skill = GetComponent<PlayerSkill>();
        mainCamera = Camera.main;
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        renderer = rendererInit;
        playerHealth.Init();
        stickControllerRect = stickController.GetComponent<RectTransform>();
        stickControllerInitPos = stickControllerRect.anchoredPosition;

        skillButton.onClick.AddListener(() => OnClickSkillButton());
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
            if(!isSkill && !skill.isOneHand || skill.isOneHand) playerMove.Move();
            //����
            playerMove.Inertia(isDamage);
            //�X�e�B�b�N
            Stick();
            //�ړ�����������
            playerMove.LookMoveDirection();
        }

        //skill
        if (isSkill)
        {
            if (canSkillLine && isInteracting)
            {
                skill.SkillTouchMove(currentPosition);
                // �v���C���[���l�p�`�̒��ɐ���
                ConstrainPlayer();
            }
        }
        else
        {
            //attack
            if (!isAttack)
            {
                isAttack = true;
                playerAttack.Attack();

                Invoke("StopAttack", playerAttack.GetAttackCooldownTime());
            }
        }

        // Skill�����\���ɃG�t�F�N�g�\��
        if (!isSkill)
        {
            if (skill.coolTime >= 1)
            {
                skillChargeEffect.SetActive(true);
                canSkill = true;
            }
            else
            {
                skill.AddSkillCoolTime();
            }
        }
        else
        {
            if (skill.coolTime <= 0)
            {
                StopSkill();
            }
        }
        UpdateSkillChargeImagePosition();

        if (!levelUpEffect.IsAlive())
        {
            levelUpEffect.Stop();
        }
    }

    private void UpdateSkillChargeImagePosition()
    {
        float y = skill.coolTime * maxSkillChargeImageHeight;
        skillGaugeImage.anchoredPosition = new Vector2(skillGaugeImage.anchoredPosition.x, y);
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
        if (gameManager.state != GameManager.GameState.Game || !canControl) return;

        startPosition = position;
        currentPosition = startPosition;
        isInteracting = true;

        if (skill.isOneHand)
        {
            // �X�e�B�b�N�w�i���w��ʒu��
            stickController.transform.position = startPosition;

            // �X�e�B�b�N�����m�u�i�q�I�u�W�F�N�g�j�𓯂��ʒu��
            stickController.transform.GetChild(0).position = startPosition;
        }
        else
        {
            if (!isSkill)
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
        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        currentPosition = position;
        playerMove.UpdateMoveDir(currentPosition, startPosition);
    }

    // �^�b�`�܂��̓}�E�X����̏I��
    private void OnTouchEnd()
    {
        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        if (isSkill) StopSkill();
        isInteracting = false;
        playerMove.Init();
        InitializeStick();
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

    void Skill()
    {
        if (isSkill) return;

        isSkill = true;
        //�O�̂��߃{�^��������������Ɛ����������߂̎w�𗣂������肪���Ԃ�Ȃ��悤�ɑ҂�
        Invoke("StartLine", 1f);
        centerToGrayEffect.Gray(true);
        skillChargeEffect.SetActive(false);
        if (!skill.isOneHand) stickController.SetActive(false);
        canSkill = false;
        //maxSpeed *= skillAddSpeed;
        //moveSpeed *= skillAddSpeed;
        // �Փ˖�����
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Invoke("StopSkill", maxSkillTime);
    }

    void StartLine()
    {
        canSkillLine = true;
    }

    void StopSkill()
    {
        // �������łɃX�L����ԂłȂ���΁A�������Ȃ�
        if (!isSkill || isSkillEndEffect) return;

        int enemyNum = skill.SkillTouchEnded();
        canSkillLine = false;
        isSkillEndEffect = true;

        if(enemyNum > 0)
        {
            playerSkillAnim.PlayerSkillAnimPlay(() =>
            {
                StopSkillAnim();
            });
        }
        else
        {
            StopSkillAnim();
        }
    }

    void OnClickSkillButton()
    {
        if (!isSkill && skill.coolTime >= 1)
        {
            // �J�����̎l������n�ʂւ̌�_���擾
            CalculateCorners();
            Skill();
        }
    }

    void StopSkillAnim()
    {
        // �X�L���I������
        isSkill = false;
        isSkillEndEffect = false;
        //maxSpeed /= skillAddSpeed;
        //moveSpeed /= skillAddSpeed;
        centerToGrayEffect.Gray(false);
        if (!skill.isOneHand) {
            stickController.SetActive(true); 
        }
        // �Փ˂��ĂїL���ɂ���
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        canSkillLine = true;
        OnTouchEnd();//�X�L���I�����ɋ����I�Ƀ^�b�`����
    }

    //�X�L�����t���O�擾
    public bool GetIsSkill() { return isSkill; }

    //�������牺�̓J�����͈͓̔��Ɉړ��͈͂𐧌�
    // �J�����̎l������n�ʂւ̌�_���擾
    void CalculateCorners()
    {
        // �J�����̃r���[�̎l����Viewport���W
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(leftOffset, topOffset, mainCamera.nearClipPlane), // ����
            new Vector3(rightOffset, topOffset, mainCamera.nearClipPlane), // �E��
            new Vector3(rightOffset, bottomOffset, mainCamera.nearClipPlane), // �E��
            new Vector3(leftOffset, bottomOffset, mainCamera.nearClipPlane)  // ����
        };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);
            Vector3 direction = (worldPoint - mainCamera.transform.position).normalized;

            // �n�ʂƂ̌�_���擾
            if (Physics.Raycast(mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                corners[i] = hit.point;
            }
        }
    }

    void ConstrainPlayer()
    {
        // �v���C���[�̌��݈ʒu
        Vector3 playerPosition = transform.position;

        // �l�p�`�̒��_���g����2D���ʏ�Ő����������
        Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);

        // �l�p�`��2D���ʏ�Œ�`
        Vector2[] polygon = new Vector2[]
        {
            new Vector2(corners[0].x, corners[0].z), // ����
            new Vector2(corners[1].x, corners[1].z), // �E��
            new Vector2(corners[2].x, corners[2].z), // �E��
            new Vector2(corners[3].x, corners[3].z)  // ����
        };

        // �v���C���[���l�p�`�̊O�ɏo�Ă��邩
        if (!IsPointInPolygon(player2D, polygon))
        {
            // �v���C���[���l�p�`�̊O�ɏo�Ă���΍ł��߂��_�ɐ���
            Vector2 closestPoint = FindClosestPointInPolygon(player2D, polygon);
            transform.position = new Vector3(closestPoint.x, transform.position.y, closestPoint.y);
        }
    }

    // �v���C���[���l�p�`�̊O�ɏo�Ă��邩����
    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int count = polygon.Length;// ���p�`�̒��_�̐�
        bool isInside = false;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            // ���_i�ƒ��_j�̕ӂ��l����
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                isInside = !isInside; // ��������𔽓]������
            }
        }
        return isInside;
    }

    //�v���C���[�����ԋ߂��_���擾�i�e�ӂň�ԋ߂��_��T�����̒��ň�ԋ߂��_��Ԃ��j
    Vector2 FindClosestPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        Vector2 closestPoint = polygon[0];// �����l�Ƃ��đ��p�`�̍ŏ��̒��_��ݒ�
        float minDistance = float.MaxValue;// �ŏ������𖳌���ŏ�����

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 segmentStart = polygon[i];
            Vector2 segmentEnd = polygon[(i + 1) % polygon.Length];// ���̒��_�i���[�v�����j

            // �ӏ�̍ł��߂��_��T��
            Vector2 closestOnSegment = ClosestPointOnSegment(point, segmentStart, segmentEnd);

            // �_�Ƃ̋������v�Z
            float distance = Vector2.Distance(point, closestOnSegment);

            // �ŏ��������X�V
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestOnSegment;
            }
        }

        return closestPoint;
    }

    //�v���C���[�̈ʒu����segment�̐�����ɐ����ɓ��e���ꂽ�ʒu���擾
    Vector2 ClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;// �����̃x�N�g��
        float t = Vector2.Dot(point - segmentStart, segment) / segment.sqrMagnitude;// �ˉe�̌W��

        // t��0�`1�͈̔͂ɃN�����v
        t = Mathf.Clamp01(t);

        // ������̍ł��߂��_��Ԃ�
        return segmentStart + t * segment;
    }

    public void Init()
    {
        skillChargeEffect.SetActive(false);
        canSkill = false;
        isInteracting = false;
        playerMove.Init();
        currentPosition = Vector2.zero;
        transform.position = Vector2.zero;
        InitializeStick();
        StopSkill();
        playerHealth.Init();
        canSkillLine = false;
        skill.ResetSkillCoolTime();
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

    public void SetSkillChargeImage(RectTransform _skillGaugeImage)
    {
        skillGaugeImage = _skillGaugeImage;
    }

    public void SetSkillButton(Button _skillButton)
    {
        if (skillButton != null)
        {
            // �O�̃{�^���̃��X�i�[���폜
            skillButton.onClick.RemoveAllListeners();
        }

        skillButton = _skillButton;
        skillButton.onClick.AddListener(() => OnClickSkillButton());
    }

    public int GetHP() => playerHealth.GetHp();
}