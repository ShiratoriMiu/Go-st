using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInputAction action;
    Rigidbody rb;
    Vector3 velocity;

    //攻撃中か
    bool isAttack = false;
    bool isDamage = false;

    //移動
    [SerializeField]
    float moveSpeed = 1f;
    [SerializeField]
    float maxSpeed = 10f;
    [SerializeField]
    float damping = 0.98f; // 減衰率（1に近いほどゆっくり減衰）

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 moveDirection;
    private bool isInteracting = false;

    [SerializeField]
    GameObject stickPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]の中のActionに紐づくイベントリスナーを登録
        action.Player.Touch.performed += OnTouchMoved;
        action.Player.TouchClick.started += OnTouchStarted;
        action.Player.TouchClick.canceled += OnTouchEnded;
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
}
