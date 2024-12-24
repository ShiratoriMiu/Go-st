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
        // ActionMaps[Player]�̒���Action�ɕR�Â��C�x���g���X�i�[��o�^
        action.Player.Touch.performed += OnTouchMoved;
        action.Player.TouchClick.started += OnTouchStarted;
        action.Player.TouchClick.canceled += OnTouchEnded;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isInteracting)
        {
            //�ړ�
            Move();
            //����
            Inertia();

            Stick();
        }
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

    // �^�b�`�܂��̓}�E�X�̊J�n�ʒu
    public void OnTouchStarted(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isInteracting = true;
            //�ŏ��Ƀ^�b�`�܂��̓N���b�N�����ꏊ��ۑ�
            // �^�b�`�f�o�C�X���A�N�e�B�u�Ȃ�^�b�`���W���擾
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                startPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                // ����ȊO�̓}�E�X���W���擾
                startPosition = Mouse.current.position.ReadValue();
            }
            //�X�^�[�g�|�W�V�����ȊO���ݒ肷�邱�ƂőO��̈ʒu�����Z�b�g
            currentPosition = startPosition;
            stickPrefab.SetActive(true);
            stickPrefab.transform.position = startPosition;
            stickPrefab.transform.GetChild(0).gameObject.transform.position = startPosition;
        }
    }

    // �^�b�`�܂��̓}�E�X�̌��݈ʒu
    public void OnTouchMoved(InputAction.CallbackContext context)
    {
        if (context.performed && isInteracting)
        {
            currentPosition = context.ReadValue<Vector2>();
            moveDirection = (currentPosition - startPosition).normalized;
        }
    }

    // �^�b�`�܂��̓}�E�X����̏I��
    public void OnTouchEnded(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isInteracting = false;
            moveDirection = Vector2.zero;
            stickPrefab.SetActive(false);
        }
    }

    //�ړ�update
    void Move()
    {
        Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
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
        else if (moveDirection == Vector2.zero || isAttack == true)
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

    void Stick()
    {
        //�X�e�B�b�N�̈ʒu���^�b�`�����ʒu���痣�ꂷ���Ȃ��悤��
        Vector2 stickPos = Vector2.ClampMagnitude(currentPosition - startPosition, 50);
        stickPrefab.transform.GetChild(0).gameObject.transform.position = startPosition + stickPos;
    }
}
