using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    //�ړ�
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float damping = 0.98f; // �������i1�ɋ߂��قǂ�����茸���j
    [SerializeField] float attackSpeed;//�U���̑��x��

    [SerializeField] int maxHP = 10;

    [SerializeField] GameObject stickPrefab;//���z�X�e�B�b�N
    [SerializeField] GameObject attackPrefab;    //���˂������v���n�u

    PlayerInputAction action;
    Rigidbody rb;
    Vector3 velocity;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 moveDirection;

    private bool isInteracting = false;//���͒��t���O
    private bool isAttack = false;//�U�����t���O
    private bool isDamage = false;
    private bool isSkill = false;//�K�E�Z�������t���O

    private float touchTime = 0;
    private float hp = 0;
    //�I�[�g�G�C���p�p�x�ϐ�
    private float degreeAttack = 0.0f;
    private float radAttack = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new PlayerInputAction();
        // ActionMaps[Player]�̒���Action�ɕR�Â��C�x���g���X�i�[��o�^
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
            //�ړ�
            Move();
            //����
            Inertia();
            //�X�e�B�b�N
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
            //0.5�b�ȓ��Ƀ^�b�`�����|�C���g����100�����ƕK�E�Z����
            if(touchTime < 0.5f && (currentPosition - startPosition).magnitude > 100)
            {
                print("�K�E�Z");
            }
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

    void Attack()
    {
        if (isAttack == false)
        {
            isAttack = true;

            //  �^�OEnemy�̃I�u�W�F�N�g�����ׂĎ擾���A10f�ȓ��̍ł��߂��G�l�~�[���擾����B
            GameObject nearestEnemy = null;    //�O��̍U���ň�ԋ߂������G�����Z�b�g
            float minDis = 10f;    //�I�[�g�G�C���͈́B���D�݂ŁB
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");//Enemy�^�O�������I�u�W�F�N�g�����ׂĔz��Ɋi�[�B
            foreach (GameObject enemy in enemys)    //�SEnemy�I�u�W�F�N�g����z����ЂƂÂ��[�v�B
            {
                float dis = Vector3.Distance(transform.position, enemy.transform.position);    //�v���C���[�L�����ƃ��[�v���̓G�I�u�W�F�N�g�̋����������Z���č����o���B
                if (dis < minDis)    //�I�[�g�G�C���͈�(10f)�ȓ����m�F
                {
                    minDis = dis;    //����Ƃ���ԋ߂��G�Ƃ̋����X�V�B���̃��[�v�p�B
                    nearestEnemy = enemy;    //����Ƃ���ԋ߂��G�I�u�W�F�N�g�X�V�B
                }
            }
            // foreach�@���I��������AnearestEnemy�Ƀv���C���[�L���������ԋ߂��G�������Ă�B

            if (nearestEnemy != null)  //�I�[�g�G�C���L��null�`�F�b�N�B10f�ȓ��Ƀ^�OEnemy���݁B
            {
                GameObject attackObj = Instantiate(attackPrefab, this.transform.position, Quaternion.identity);
                Rigidbody rb = attackObj.GetComponent<Rigidbody>();
                Vector3 direction = (nearestEnemy.transform.position - this.transform.position).normalized;
                rb.velocity = direction * attackSpeed;

                Invoke("StopAttack", 0.4f);    //�A�˂�h�����߂̃t���O����B
            }
        }
    }

    void StopAttack()
    {
        isAttack = false;    //�U�����t���O���낷
    }
}
