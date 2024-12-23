using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    PlayerInputAction action;
    Rigidbody rb;
    Vector3 initScale;
    Vector3 velocity;
    Vector2 inputValue;

    //接地判定
    bool isGround = false;
    //攻撃中か
    bool isAttack = false;
    bool isDamage = false;

    //移動
    [SerializeField]
    float moveSpeed = 1f;
    [SerializeField]
    float maxSpeed = 10f;
    //ジャンプ力
    [SerializeField]
    float jumpPower = 1f;
    [SerializeField]
    float damping = 0.98f; // 減衰率（1に近いほどゆっくり減衰）
    //重力調整
    [SerializeField]
    Vector2 gravity = new Vector3(0, -9.81f);

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]の中の[Jump]というActionに紐づくイベントリスナーを登録
        action.Player.Move.performed += OnMovePerformed;
        action.Player.Move.canceled += OnMoveCanceled;
        //action.Player.Jump.performed += OnJumpPerformed;
        //action.Player.Attack.performed += OnAttackPerformed;
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //重力
        Gravity();

        //移動
        Move();
        //慣性
        Inertia();
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

    //移動
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isDamage) return;
        inputValue = context.ReadValue<Vector2>();
        //走るアニメーション
        //playerAnim.Run(true);
        //進行方向を向く
        Vector3 scale = initScale;
        scale.x *= Mathf.Sign(inputValue.x);
        transform.localScale = scale;
    }

    //移動終了
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        //移動しないように
        inputValue = Vector2.zero;
        //playerAnim.Run(false);
    }

    //ジャンプ
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isGround || isDamage) return;
        //rb2d.AddForce(Vector3.up * jumpPower, ForceMode2D.Impulse);
        //playerAnim.Jump(true);
    }

    //移動update
    void Move()
    {
        Vector3 moveDirection = new Vector3(inputValue.x, 0, inputValue.y).normalized;
        rb.AddForce(moveDirection * moveSpeed, ForceMode.Force);
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
        else if (inputValue == Vector2.zero || isAttack == true)
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

    //重力
    void Gravity()
    {
        rb.AddForce(gravity, ForceMode.Force);
    }

    //攻撃
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (isDamage) return;
        isAttack = true;
        //playerAnim.Attack(true);
    }

    //ダメージ
    public void Damage()
    {
        //if (playerHP.GetIsDead()) return;
        //isDamage = true;
        //playerAnim.Hit();
        //playerHP.HPDamage();
    }

    //接地判定
    private void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
        {
            isGround = true;
            //playerAnim.Jump(true);
        }
    }

    //接地判定
    private void OnTriggerExit2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
        {
            isGround = false;
        }
    }

    //攻撃終了
    public void SetAttackFalse()
    {
        isAttack = false;
    }

    //ダメージアニメーション終了
    public void SetHitFalse()
    {
        isDamage = false;
    }
}
