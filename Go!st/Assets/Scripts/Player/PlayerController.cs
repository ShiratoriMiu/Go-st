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
    [SerializeField] float maxSkillTime = 7f; // 必殺技の最大継続時間
    [SerializeField,Header("ゲージの高さ")] private float maxSkillChargeImageHeight = 400f;

    [SerializeField] GameObject stickController;//仮想スティック
    [SerializeField] GameObject skillChargeEffect; //スキル発動可能エフェクト 
    [SerializeField] LevelUpText levelUpText;

    //画面内に移動範囲を制限
    [SerializeField] LayerMask groundLayer; // 地面のレイヤー

    [SerializeField, Range(0f, 1f)] float leftOffset = 0.1f;   // 左端のオフセット
    [SerializeField, Range(0f, 1f)] float rightOffset = 0.9f;  // 右端のオフセット
    [SerializeField, Range(0f, 1f)] float topOffset = 0.9f;    // 上端のオフセット
    [SerializeField, Range(0f, 1f)] float bottomOffset = 0.1f; // 下端のオフセット

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
    Camera mainCamera; // 使用するカメラ
    RectTransform stickControllerRect;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 stickControllerInitPos;

    private Vector3 velocity;
    private Vector3[] corners = new Vector3[4]; // 四角形の頂点

    private bool isInteracting = false;//入力中フラグ
    private bool isAttack = false;//攻撃中フラグ
    private bool isDamage = false;
    private bool isSkill = false;//必殺技フラグ
    private bool isSkillEndEffect = false;//必殺技フラグ
    private bool onAutoAim = false;
    private bool canSkillLine = false;//スキルの線を引ける状態か
    private bool canControl = false; // プレイヤー操作可能かどうか

    private float touchTime = 0;

    //スキル発動中は敵と当たらなくする
    private int playerLayer;
    private int enemyLayer;

    //リファクタリング後
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
        // イベント登録
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
            //移動
            if(!isSkill && !skill.isOneHand || skill.isOneHand) playerMove.Move();
            //慣性
            playerMove.Inertia(isDamage);
            //スティック
            Stick();
            //移動方向を向く
            playerMove.LookMoveDirection();
        }

        //skill
        if (isSkill)
        {
            if (canSkillLine && isInteracting)
            {
                skill.SkillTouchMove(currentPosition);
                // プレイヤーを四角形の中に制限
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

        // Skill発動可能時にエフェクト表示
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
        if (gameManager.state != GameManager.GameState.Game || !canControl) return;

        startPosition = position;
        currentPosition = startPosition;
        isInteracting = true;

        if (skill.isOneHand)
        {
            // スティック背景を指定位置へ
            stickController.transform.position = startPosition;

            // スティック内部ノブ（子オブジェクト）を同じ位置へ
            stickController.transform.GetChild(0).position = startPosition;
        }
        else
        {
            if (!isSkill)
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
        if (gameManager.state != GameManager.GameState.Game || !canControl || !isInteracting) return;

        currentPosition = position;
        playerMove.UpdateMoveDir(currentPosition, startPosition);
    }

    // タッチまたはマウス操作の終了
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
        //スティックの位置がタッチした位置から離れすぎないように
        Vector2 stickPos = Vector2.ClampMagnitude(currentPosition - startPosition, 50);
        stickController.transform.GetChild(0).gameObject.transform.position = startPosition + stickPos;
    }

    void StopAttack()
    {
        isAttack = false;    //攻撃中フラグ下ろす
    }

    void Skill()
    {
        if (isSkill) return;

        isSkill = true;
        //念のためボタンを押した判定と線を引くための指を離した判定がかぶらないように待つ
        Invoke("StartLine", 1f);
        centerToGrayEffect.Gray(true);
        skillChargeEffect.SetActive(false);
        if (!skill.isOneHand) stickController.SetActive(false);
        canSkill = false;
        //maxSpeed *= skillAddSpeed;
        //moveSpeed *= skillAddSpeed;
        // 衝突無効化
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Invoke("StopSkill", maxSkillTime);
    }

    void StartLine()
    {
        canSkillLine = true;
    }

    void StopSkill()
    {
        // もしすでにスキル状態でなければ、何もしない
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
            // カメラの四隅から地面への交点を取得
            CalculateCorners();
            Skill();
        }
    }

    void StopSkillAnim()
    {
        // スキル終了処理
        isSkill = false;
        isSkillEndEffect = false;
        //maxSpeed /= skillAddSpeed;
        //moveSpeed /= skillAddSpeed;
        centerToGrayEffect.Gray(false);
        if (!skill.isOneHand) {
            stickController.SetActive(true); 
        }
        // 衝突を再び有効にする
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        canSkillLine = true;
        OnTouchEnd();//スキル終了時に強制的にタッチ解除
    }

    //スキル中フラグ取得
    public bool GetIsSkill() { return isSkill; }

    //ここから下はカメラの範囲内に移動範囲を制限
    // カメラの四隅から地面への交点を取得
    void CalculateCorners()
    {
        // カメラのビューの四隅のViewport座標
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(leftOffset, topOffset, mainCamera.nearClipPlane), // 左上
            new Vector3(rightOffset, topOffset, mainCamera.nearClipPlane), // 右上
            new Vector3(rightOffset, bottomOffset, mainCamera.nearClipPlane), // 右下
            new Vector3(leftOffset, bottomOffset, mainCamera.nearClipPlane)  // 左下
        };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);
            Vector3 direction = (worldPoint - mainCamera.transform.position).normalized;

            // 地面との交点を取得
            if (Physics.Raycast(mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                corners[i] = hit.point;
            }
        }
    }

    void ConstrainPlayer()
    {
        // プレイヤーの現在位置
        Vector3 playerPosition = transform.position;

        // 四角形の頂点を使って2D平面上で制約をかける
        Vector2 player2D = new Vector2(playerPosition.x, playerPosition.z);

        // 四角形を2D平面上で定義
        Vector2[] polygon = new Vector2[]
        {
            new Vector2(corners[0].x, corners[0].z), // 左上
            new Vector2(corners[1].x, corners[1].z), // 右上
            new Vector2(corners[2].x, corners[2].z), // 右下
            new Vector2(corners[3].x, corners[3].z)  // 左下
        };

        // プレイヤーが四角形の外に出ているか
        if (!IsPointInPolygon(player2D, polygon))
        {
            // プレイヤーが四角形の外に出ていれば最も近い点に制限
            Vector2 closestPoint = FindClosestPointInPolygon(player2D, polygon);
            transform.position = new Vector3(closestPoint.x, transform.position.y, closestPoint.y);
        }
    }

    // プレイヤーが四角形の外に出ているか判定
    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int count = polygon.Length;// 多角形の頂点の数
        bool isInside = false;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            // 頂点iと頂点jの辺を考える
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                isInside = !isInside; // 内部判定を反転させる
            }
        }
        return isInside;
    }

    //プレイヤーから一番近い点を取得（各辺で一番近い点を探しその中で一番近い点を返す）
    Vector2 FindClosestPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        Vector2 closestPoint = polygon[0];// 初期値として多角形の最初の頂点を設定
        float minDistance = float.MaxValue;// 最小距離を無限大で初期化

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 segmentStart = polygon[i];
            Vector2 segmentEnd = polygon[(i + 1) % polygon.Length];// 次の頂点（ループ処理）

            // 辺上の最も近い点を探す
            Vector2 closestOnSegment = ClosestPointOnSegment(point, segmentStart, segmentEnd);

            // 点との距離を計算
            float distance = Vector2.Distance(point, closestOnSegment);

            // 最小距離を更新
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestOnSegment;
            }
        }

        return closestPoint;
    }

    //プレイヤーの位置からsegmentの線分上に垂直に投影された位置を取得
    Vector2 ClosestPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;// 線分のベクトル
        float t = Vector2.Dot(point - segmentStart, segment) / segment.sqrMagnitude;// 射影の係数

        // tを0～1の範囲にクランプ
        t = Mathf.Clamp01(t);

        // 線分上の最も近い点を返す
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

    public void SetSkillChargeImage(RectTransform _skillGaugeImage)
    {
        skillGaugeImage = _skillGaugeImage;
    }

    public void SetSkillButton(Button _skillButton)
    {
        if (skillButton != null)
        {
            // 前のボタンのリスナーを削除
            skillButton.onClick.RemoveAllListeners();
        }

        skillButton = _skillButton;
        skillButton.onClick.AddListener(() => OnClickSkillButton());
    }

    public int GetHP() => playerHealth.GetHp();
}