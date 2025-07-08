using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    //移動
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float damping = 0.98f; // 減衰率（1に近いほどゆっくり減衰）

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

    //移動update
    public void Move()
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
    public void Inertia(bool _isDamage)
    {
        if (_isDamage)
        {
            rb.velocity = Vector2.zero;
        }
        //入力がなくなった時または攻撃中にX軸の慣性の調整
        else if (moveDirection == Vector2.zero)
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

    public void LookMoveDirection()
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
