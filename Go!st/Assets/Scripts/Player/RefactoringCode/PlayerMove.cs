using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    //�ړ�
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float damping = 0.98f; // �������i1�ɋ߂��قǂ�����茸���j

    private Vector2 moveDirection;

    private Vector3 velocity;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init()
    {
        moveDirection = Vector2.zero;
    }

    //�ړ�update
    public void Move()
    {
        Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
        //���������̈ړ����ő�X�s�[�h�𒴂��Ȃ��悤��
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // ���������̂݌v�Z
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
    }

    //����
    public void Inertia(bool _isDamage)
    {
        if (_isDamage)
        {
            rb.velocity = Vector2.zero;
        }
        //���͂��Ȃ��Ȃ������܂��͍U������X���̊����̒���
        else if (moveDirection == Vector2.zero)
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

    public void LookMoveDirection()
    {
        Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.y);
        // �ړ��������[���łȂ��ꍇ�ɉ�]
        if (moveDir.magnitude > 0.1f)
        {
            // �ړ������Ɍ���
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void UpdateMoveDir(Vector2 _currentPosition, Vector2 _startPosition)
    {
        moveDirection = (_currentPosition - _startPosition).normalized;
    }

    public void AddSpeed(float _addSpeed)
    {
        moveSpeed *= _addSpeed;
    }

    public void RemoveSpeed(float _removeSpeed)
    {
        moveSpeed /= _removeSpeed;
    }
}
