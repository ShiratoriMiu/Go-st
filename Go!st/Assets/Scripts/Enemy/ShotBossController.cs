using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotBossController : EnemyBase
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField, Header("一度に発射する弾の数")] int bulletNum = 4;
    [SerializeField, Header("敵の索敵範囲に入ってから攻撃するまでの時間")] float attackInterval = 5f;
    [SerializeField, Header("弾のスピード")] float bulletSpeed = 5f;
    [SerializeField, Header("上昇距離")] float riseDistance = 5f;

    float attackIntervalCount = 0f;
    enum State { Follow, Charge, Attack }
    State state = State.Follow;

    void FixedUpdate()
    {
        if (!isActive) return;
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != null && playerController != null)
        {
            Stan();
            if (playerController.GetIsSkill())
            {
                return;
            }
            else
            {
                if (state == State.Charge)
                {
                    attackIntervalCount += Time.deltaTime;
                    //animator.SetTrigger("isAttack");

                    if (attackIntervalCount > attackInterval)
                    {
                        attackIntervalCount = 0f; // カウントリセット
                        
                        float angleStep = 360f / bulletNum;
                        float angle = 0f;

                        for (int i = 0; i < bulletNum; i++)
                        {
                            // 水平方向の角度を設定
                            float bulletDirX = Mathf.Cos(angle * Mathf.Deg2Rad);
                            float bulletDirZ = Mathf.Sin(angle * Mathf.Deg2Rad);

                            Vector3 bulletMoveDirection = new Vector3(bulletDirX, 0, bulletDirZ).normalized;

                            GameObject bullet = Instantiate(bulletPrefab, this.transform.position, Quaternion.identity);
                            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                            if (bulletRb != null)
                            {
                                bulletRb.velocity = bulletMoveDirection * bulletSpeed;
                            }

                            angle += angleStep;
                        }
                        //animator.SetTrigger("isWait");
                        state = State.Follow; // 攻撃後はFollow状態に戻るなど必要に応じて
                    }
                }

                if (state == State.Follow)
                {
                    Vector3 direction = (player.transform.position - transform.position).normalized;
                    rb.velocity = direction * moveSpeed;
                    transform.LookAt(player.transform.position);
                }
            }
        }
    }

    private void Reset()
    {
        state = State.Follow;
        rb.isKinematic = false;
        //animator.SetTrigger("isWait");
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (state != State.Attack) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            transform.DOKill(); // DOTween 移動停止
            rb.isKinematic = false;
            state = State.Charge;
            //animator.SetTrigger("isWait");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.Follow) return;

        if (other.gameObject.CompareTag("Player"))
        {
            state = State.Charge;
        }
    }

    public override void Hidden()
    {
        base.Hidden();
        rb.WakeUp();
        Reset();
    }

    public override void Damage(int _damage)
    {
        base.Damage(_damage);

        if (hp <= 0)
        {
            Dead();
        }
    }
}
