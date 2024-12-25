using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    //移動
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float damping = 0.98f; // 減衰率（1に近いほどゆっくり減衰）
    [SerializeField] float attackSpeed;//攻撃の速度力

    [SerializeField] int maxHP = 10;

    [SerializeField] GameObject stickPrefab;//仮想スティック
    [SerializeField] GameObject attackPrefab;    //発射したいプレハブ

    PlayerInputAction action;
    Rigidbody rb;
    Vector3 velocity;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 moveDirection;

    private bool isInteracting = false;//入力中フラグ
    private bool isAttack = false;//攻撃中フラグ
    private bool isDamage = false;
    private bool isSkill = false;//必殺技発動中フラグ

    private float touchTime = 0;
    private float hp = 0;
    //オートエイム用角度変数
    private float degreeAttack = 0.0f;
    private float radAttack = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]の中のActionに紐づくイベントリスナーを登録
        action.Player.Touch.performed += OnTouchMoved;
        action.Player.TouchClick.started += OnTouchStarted;
        action.Player.TouchClick.canceled += OnTouchEnded;
        hp = maxHP;
    }

    private void Update()
    {
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
        if (isInteracting)
        {
            //移動
            Move();
            //慣性
            Inertia();
            //スティック
            Stick();
        }
    }

    private void OnEnable()
    {
        // Inputアクションを有効化
        action.Enable();
    }

    private void OnDisable()
    {
        // Inputアクションを無効化
        action.Disable();
    }

    // タッチまたはマウスの開始位置
    public void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isInteracting = true;
            //最初にタッチまたはクリックした場所を保存
            // タッチデバイスがアクティブならタッチ座標を取得
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                startPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                // それ以外はマウス座標を取得
                startPosition = Mouse.current.position.ReadValue();
            }
            //スタートポジション以外も設定することで前回の位置をリセット
            currentPosition = startPosition;
            stickPrefab.SetActive(true);
            stickPrefab.transform.position = startPosition;
            stickPrefab.transform.GetChild(0).gameObject.transform.position = startPosition;
        }
    }

    // タッチまたはマウスの現在位置
    public void OnTouchMoved(InputAction.CallbackContext context)
    {
        if (context.performed && isInteracting)
        {
            currentPosition = context.ReadValue<Vector2>();
            moveDirection = (currentPosition - startPosition).normalized;
            //0.5秒以内にタッチしたポイントから100離れると必殺技発動
            if(touchTime < 0.5f && (currentPosition - startPosition).magnitude > 100)
            {
                print("必殺技");
            }
        }
    }

    // タッチまたはマウス操作の終了
    public void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isInteracting = false;
            moveDirection = Vector2.zero;
            stickPrefab.SetActive(false);
        }
    }

    //移動update
    void Move()
    {
        Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
        //水平方向の移動が最大スピードを超えないように
        velocity = rb.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z); // 水平方向のみ計算
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
    }

    //慣性
    void Inertia()
    {
        if (isDamage)
        {
            rb.velocity = Vector2.zero;
        }
        //入力がなくなった時または攻撃中にX軸の慣性の調整
        else if (moveDirection == Vector2.zero || isAttack == true)
        {
            // 現在のvelocityを取得し減衰を適用
            velocity = rb.velocity;
            velocity.x *= damping;
            rb.velocity = velocity;

            // 一定以下の速度になったら完全に停止
            if (rb.velocity.x < 0.01f && rb.velocity.x > -0.01f)
            {
                rb.velocity = new Vector2(rb.velocity.x * damping, rb.velocity.y);
            }
        }
    }

    void Stick()
    {
        //スティックの位置がタッチした位置から離れすぎないように
        Vector2 stickPos = Vector2.ClampMagnitude(currentPosition - startPosition, 50);
        stickPrefab.transform.GetChild(0).gameObject.transform.position = startPosition + stickPos;
    }

    void Attack()
    {
        if (isAttack == false)
        {
            isAttack = true;

            //  タグEnemyのオブジェクトをすべて取得し、10f以内の最も近いエネミーを取得する。
            GameObject nearestEnemy = null;    //前回の攻撃で一番近かった敵をリセット
            float minDis = 10f;    //オートエイム範囲。お好みで。
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");//Enemyタグがついたオブジェクトをすべて配列に格納。
            foreach (GameObject enemy in enemys)    //全Enemyオブジェクト入り配列をひとつづつループ。
            {
                float dis = Vector3.Distance(transform.position, enemy.transform.position);    //プレイヤーキャラとループ中の敵オブジェクトの距離を引き算して差を出す。
                if (dis < minDis)    //オートエイム範囲(10f)以内か確認
                {
                    minDis = dis;    //今んとこ一番近い敵との距離更新。次のループ用。
                    nearestEnemy = enemy;    //今んとこ一番近い敵オブジェクト更新。
                }
            }
            // foreach　が終わった時、nearestEnemyにプレイヤーキャラから一番近い敵が入ってる。

            if (nearestEnemy != null)  //オートエイム有効nullチェック。10f以内にタグEnemy存在。
            {
                GameObject attackObj = Instantiate(attackPrefab, this.transform.position, Quaternion.identity);
                Rigidbody rb = attackObj.GetComponent<Rigidbody>();
                Vector3 direction = (nearestEnemy.transform.position - this.transform.position).normalized;
                rb.velocity = direction * attackSpeed;

                Invoke("StopAttack", 0.4f);    //連射を防ぐためのフラグ操作。
            }
        }
    }

    void StopAttack()
    {
        isAttack = false;    //攻撃中フラグ下ろす
    }
}
