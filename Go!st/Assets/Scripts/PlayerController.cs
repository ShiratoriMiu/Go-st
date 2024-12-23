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

    //�ڒn����
    bool isGround = false;
    //�U������
    bool isAttack = false;
    bool isDamage = false;

    //�ړ�
    [SerializeField]
    float moveSpeed = 1f;
    [SerializeField]
    float maxSpeed = 10f;
    //�W�����v��
    [SerializeField]
    float jumpPower = 1f;
    [SerializeField]
    float damping = 0.98f; // �������i1�ɋ߂��قǂ�����茸���j
    //�d�͒���
    [SerializeField]
    Vector2 gravity = new Vector3(0, -9.81f);

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]�̒���[Jump]�Ƃ���Action�ɕR�Â��C�x���g���X�i�[��o�^
        action.Player.Move.performed += OnMovePerformed;
        action.Player.Move.canceled += OnMoveCanceled;
        //action.Player.Jump.performed += OnJumpPerformed;
        //action.Player.Attack.performed += OnAttackPerformed;
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //�d��
        Gravity();

        //�ړ�
        Move();
        //����
        Inertia();
    }

    private void OnEnable()
    {
        // Input�A�N�V������L����
        action.Enable();
    }

    private void OnDisable()
    {
        // Input�A�N�V�����𖳌���
        action.Disable();
    }

    //�ړ�
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isDamage) return;
        inputValue = context.ReadValue<Vector2>();
        //����A�j���[�V����
        //playerAnim.Run(true);
        //�i�s����������
        Vector3 scale = initScale;
        scale.x *= Mathf.Sign(inputValue.x);
        transform.localScale = scale;
    }

    //�ړ��I��
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        //�ړ����Ȃ��悤��
        inputValue = Vector2.zero;
        //playerAnim.Run(false);
    }

    //�W�����v
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isGround || isDamage) return;
        //rb2d.AddForce(Vector3.up * jumpPower, ForceMode2D.Impulse);
        //playerAnim.Jump(true);
    }

    //�ړ�update
    void Move()
    {
        Vector3 moveDirection = new Vector3(inputValue.x, 0, inputValue.y).normalized;
        rb.AddForce(moveDirection * moveSpeed, ForceMode.Force);
        //���������̈ړ����ő�X�s�[�h�𒴂��Ȃ��悤��
        velocity = rb.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z); // ���������̂݌v�Z
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
    }

    //����
    void Inertia()
    {
        if (isDamage)
        {
            rb.velocity = Vector2.zero;
        }
        //���͂��Ȃ��Ȃ������܂��͍U������X���̊����̒���
        else if (inputValue == Vector2.zero || isAttack == true)
        {
            // ���݂�velocity���擾��������K�p
            velocity = rb.velocity;
            velocity.x *= damping;
            rb.velocity = velocity;

            // ���ȉ��̑��x�ɂȂ����犮�S�ɒ�~
            if (rb.velocity.x < 0.01f && rb.velocity.x > -0.01f)
            {
                rb.velocity = new Vector2(rb.velocity.x * damping, rb.velocity.y);
            }
        }
    }

    //�d��
    void Gravity()
    {
        rb.AddForce(gravity, ForceMode.Force);
    }

    //�U��
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (isDamage) return;
        isAttack = true;
        //playerAnim.Attack(true);
    }

    //�_���[�W
    public void Damage()
    {
        //if (playerHP.GetIsDead()) return;
        //isDamage = true;
        //playerAnim.Hit();
        //playerHP.HPDamage();
    }

    //�ڒn����
    private void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
        {
            isGround = true;
            //playerAnim.Jump(true);
        }
    }

    //�ڒn����
    private void OnTriggerExit2D(Collider2D _other)
    {
        if (_other.CompareTag("Ground"))
        {
            isGround = false;
        }
    }

    //�U���I��
    public void SetAttackFalse()
    {
        isAttack = false;
    }

    //�_���[�W�A�j���[�V�����I��
    public void SetHitFalse()
    {
        isDamage = false;
    }
}
