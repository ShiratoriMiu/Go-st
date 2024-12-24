using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_old : MonoBehaviour
{
    PlayerInputAction action;
    Rigidbody rb;
    Vector3 velocity;
    Vector2 inputValue;

    //�U������
    bool isAttack = false;
    bool isDamage = false;

    //�ړ�
    [SerializeField]
    float moveSpeed = 1f;
    [SerializeField]
    float maxSpeed = 10f;
    [SerializeField]
    float damping = 0.98f; // �������i1�ɋ߂��قǂ�����茸���j

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]�̒���Action�ɕR�Â��C�x���g���X�i�[��o�^
        action.Player.Move.performed += OnMovePerformed;
        action.Player.Move.canceled += OnMoveCanceled;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
    }

    //�ړ��I��
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        //�ړ����Ȃ��悤��
        inputValue = Vector2.zero;
        //playerAnim.Run(false);
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

    //�_���[�W
    public void Damage()
    {
        //if (playerHP.GetIsDead()) return;
        //isDamage = true;
        //playerAnim.Hit();
        //playerHP.HPDamage();
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
