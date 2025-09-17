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
    //移動
    [SerializeField] float skillAddSpeed = 1.5f;//必殺技中にスピードを上げる量

    [SerializeField,Header("ゲージの高さ")] private float maxSkillChargeImageHeight = 400f;

    [SerializeField] GameObject stickController;//仮想スティック

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

    private bool isInteracting = false;//入力中フラグ
    private bool isAttack = false;//攻撃中フラグ
    private bool isDamage = false;

    private bool onAutoAim = false;
    private bool canControl = false; // プレイヤー操作可能かどうか

    private float touchTime = 0;

    //リファクタリング後
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] PlayerSkill playerSkill;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerMove playerMove;

    //着せ替え時のキャラ回転
    [SerializeField] private float rotationSpeed = 0;
    private bool skinChangeIsInteracting = false;
    private Vector2 lastTouchPos;
    private Vector2 currentTouchPos;

    private float rotationVelocity = 0f;   // 現在の回転速度
    [SerializeField] private float rotationDamping = 5f; // 減速の速さ


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
        // イベント登録
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
                // 慣性で回転
                if (Mathf.Abs(rotationVelocity) > 0.001f)
                {
                    transform.Rotate(0f, -rotationVelocity * Time.deltaTime, 0f, Space.World);

                    // 減速 (exp 減衰)
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
            //移動
            if(!playerSkill.GetIsSkill() && !playerSkill.isOneHand || playerSkill.isOneHand) playerMove.Move();
            //慣性
            playerMove.Inertia(isDamage);
            //スティック
            Stick();
            //移動方向を向く
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
            rotationVelocity = deltaX * rotationSpeed; // velocityとして保持
            transform.Rotate(0f, -rotationVelocity * Time.deltaTime, 0f, Space.World);
        }

        lastTouchPos = currentTouchPos;
    }


    private void OnDisable()
    {
        // Inputアクションを無効化
        OnDestroy();
    }

    private void OnDestroy()
    {
        // イベント解除
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTouchStart -= OnTouchStart;
            InputManager.Instance.OnTouchMove -= OnTouchMove;
            InputManager.Instance.OnTouchEnd -= OnTouchEnd;
        }
    }

    // タッチまたはマウスの開始位置
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
            // スティック背景を指定位置へ
            stickController.transform.position = startPosition;

            // スティック内部ノブ（子オブジェクト）を同じ位置へ
            stickController.transform.GetChild(0).position = startPosition;
        }
        else
        {
            if (!playerSkill.GetIsSkill())
            {
                // スティック背景を指定位置へ
                stickController.transform.position = startPosition;

                // スティック内部ノブ（子オブジェクト）を同じ位置へ
                stickController.transform.GetChild(0).position = startPosition;
            }
        }
    }

    // タッチまたはマウスの現在位置
    private void OnTouchMove(Vector2 position)
    {
        if (gameManager.state == GameManager.GameState.SkinChange && skinChangeIsInteracting)
        {
            currentTouchPos = position; // 位置だけ記録
            return;
        }

        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        currentPosition = position;
        playerMove.UpdateMoveDir(currentPosition, startPosition);
    }

    // タッチまたはマウス操作の終了
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
            playerSkill.StopSkill(startPosition, InitializeStick); // コールバックで渡す
        }
        else
        {
            InitializeStick(); // スキル中でなければ即戻す
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
        //スティックの位置がタッチした位置から離れすぎないように
        Vector2 stickPos = Vector2.ClampMagnitude(currentPosition - startPosition, 50);
        stickController.transform.GetChild(0).gameObject.transform.position = startPosition + stickPos;
    }

    void StopAttack()
    {
        isAttack = false;    //攻撃中フラグ下ろす
    }

    //スキル中フラグ取得
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
            // 速度アップ
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
        Debug.Log($"速度アップ：{multiplier}倍");

        yield return new WaitForSeconds(duration);

        playerMove.RemoveSpeed(multiplier);
        Debug.Log("速度元に戻った！");
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