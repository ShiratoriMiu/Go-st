using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    //移動
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float damping = 0.98f; // 減衰率（1に近いほどゆっくり減衰）
    [SerializeField] float attackSpeed;//攻撃の速度力
    [SerializeField] float attackDis = 10f;    //オートエイム範囲。お好みで。

    [SerializeField] int maxHP = 10;

    [SerializeField] GameObject stickPrefab;//仮想スティック
    [SerializeField] GameObject attackPrefab;    //発射したいプレハブ

    PlayerInputAction action;
    Rigidbody rb;
    Vector3 velocity;
    PlayerSkill skill;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 moveDirection;

    private bool isInteracting = false;//入力中フラグ
    private bool isAttack = false;//攻撃中フラグ
    private bool isDamage = false;
    private bool isSkill = false;//必殺話字フラグ

    private float touchTime = 0;
    private float hp = 0;
    //オートエイム用角度変数
    private float degreeAttack = 0.0f;
    private float radAttack = 0.0f;
    private float nearestEnemyDis;

    //画面内に移動範囲を制限
    [SerializeField] LayerMask groundLayer; // 地面のレイヤー

    [SerializeField, Range(0f, 1f)] float leftOffset = 0.1f;   // 左端のオフセット
    [SerializeField, Range(0f, 1f)] float rightOffset = 0.9f;  // 右端のオフセット
    [SerializeField, Range(0f, 1f)] float topOffset = 0.9f;    // 上端のオフセット
    [SerializeField, Range(0f, 1f)] float bottomOffset = 0.1f; // 下端のオフセット

    Camera mainCamera; // 使用するカメラ

    private Vector3[] corners = new Vector3[4]; // 四角形の頂点

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        skill = GetComponent<PlayerSkill>();
        action = new PlayerInputAction();
        // ActionMaps[Player]の中のActionに紐づくイベントリスナーを登録
        action.Player.Touch.performed += OnTouchMoved;
        action.Player.TouchClick.started += OnTouchStarted;
        action.Player.TouchClick.canceled += OnTouchEnded;
        hp = maxHP;
        mainCamera = Camera.main;
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
            //移動方向を向く
            LookMoveDirection();
        }

        if (isSkill)
        {
            skill.SkillTouchMove();
            // プレイヤーを四角形の中に制限
            ConstrainPlayer();
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
            isInteracting = TouchAreaJudge(startPosition);
            if (isInteracting)
            {
                //スタートポジション以外も設定することで前回の位置をリセット
                currentPosition = startPosition;
                stickPrefab.SetActive(true);
                stickPrefab.transform.position = startPosition;
                stickPrefab.transform.GetChild(0).gameObject.transform.position = startPosition;
            }
        }
    }

    // タッチまたはマウスの現在位置
    public void OnTouchMoved(InputAction.CallbackContext context)
    {
        if (context.performed && isInteracting)
        {
            currentPosition = context.ReadValue<Vector2>();
            moveDirection = (currentPosition - startPosition).normalized;
        }
    }

    // タッチまたはマウス操作の終了
    public void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (context.canceled && isInteracting)
        {
            skill.SkillTouchEnded();
            StopSkill();

            if (touchTime < 0.2f)
            {
                //0.2秒以内にタッチしたポイントから100離れると必殺技発動
                if ((currentPosition - startPosition).magnitude > 200)
                {
                    if (!isSkill)
                    {
                        // カメラの四隅から地面への交点を取得
                        CalculateCorners();
                        Skill();
                    }
                }
                else
                {
                    nearestEnemyDis = attackDis;
                    Attack();
                }
            }
           
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
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // 水平方向のみ計算
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
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");//Enemyタグがついたオブジェクトをすべて配列に格納。
            foreach (GameObject enemy in enemys)    //全Enemyオブジェクト入り配列をひとつづつループ。
            {
                float dis = Vector3.Distance(transform.position, enemy.transform.position);    //プレイヤーキャラとループ中の敵オブジェクトの距離を引き算して差を出す。
                
                if (dis < nearestEnemyDis)    //オートエイム範囲(10f)以内か確認
                {
                    nearestEnemyDis = dis;    //今んとこ一番近い敵との距離更新。次のループ用。
                    nearestEnemy = enemy;    //今んとこ一番近い敵オブジェクト更新。
                }
            }
            // foreach　が終わった時、nearestEnemyにプレイヤーキャラから一番近い敵が入ってる。

            if (nearestEnemy != null)  //オートエイム有効nullチェック。10f以内にタグEnemy存在。
            {
                GameObject attackObj = Instantiate(attackPrefab, this.transform.position, Quaternion.identity);
                Rigidbody attackObjRb = attackObj.GetComponent<Rigidbody>();
                Vector3 attackDirection = (nearestEnemy.transform.position - this.transform.position).normalized;
                attackObjRb.velocity = attackDirection * attackSpeed;

                Invoke("StopAttack", 0.4f);    //連射を防ぐためのフラグ操作。
            }
            else
            {
                GameObject attackObj = Instantiate(attackPrefab, this.transform.position, Quaternion.identity);
                Rigidbody attackObjRb = attackObj.GetComponent<Rigidbody>();
                attackObjRb.velocity = transform.forward * attackSpeed;

                Invoke("StopAttack", 0.4f);    //連射を防ぐためのフラグ操作。
            }
        }
    }

    void StopAttack()
    {
        isAttack = false;    //攻撃中フラグ下ろす
    }

    void Skill()
    {
        isSkill = true;

        Invoke("StopSkill", 10);
    }

    void StopSkill()
    {
        skill.SkillTouchEnded();
        isSkill = false;
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
                Debug.DrawLine(mainCamera.transform.position, hit.point, Color.red); // デバッグ用ライン
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

    bool TouchAreaJudge(Vector2 _startPosition)
    {
        //RaycastAllの結果格納用のリスト作成
        List<RaycastResult> RayResult = new List<RaycastResult>();

        PointerEventData pointer = new PointerEventData(EventSystem.current);
        //PointerEvenDataに、マウスの位置をセット
        pointer.position = _startPosition;
        //RayCast（スクリーン座標）
        EventSystem.current.RaycastAll(pointer, RayResult);

        foreach (RaycastResult result in RayResult)
        {
            if (result.gameObject.CompareTag("TouchArea"))
            {
                return true;
            }
        }

        return false;
    }

    void LookMoveDirection()
    {
        Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        // 移動方向がゼロでない場合に回転
        if (moveDir.magnitude > 0.1f)
        {
            // 移動方向に向く
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
