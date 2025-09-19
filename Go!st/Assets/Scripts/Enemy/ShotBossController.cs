using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotBossController : EnemyBase
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField, Header("一度に発射する弾の数")] int bulletNum = 4;
    [SerializeField, Header("索敵後に攻撃するまでの時間")] float attackInterval = 5f;
    [SerializeField, Header("弾のスピード")] float bulletSpeed = 5f;
    [SerializeField, Header("弾の発射位置の高さ調整")] float offsetY = 0.5f;

    float attackIntervalCount = 0f;
    enum State { Follow, Charge, Attack }
    State state = State.Follow;

    void FixedUpdate()
    {
        if (!isActive || isDead) return;
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != null && playerController != null)
        {
            Stan();

            if (playerController.GetIsSkill()) return;

            if (state == State.Charge)
            {
                attackIntervalCount += Time.deltaTime;

                if (attackIntervalCount > attackInterval)
                {
                    attackIntervalCount = 0f; // カウントリセット

                    SoundManager.Instance.PlaySE("BulletEnemyShotSE");

                    float angleStep = 360f / bulletNum;
                    float angle = 0f;
                    animator.SetTrigger("isAttack");
                    for (int i = 0; i < bulletNum; i++)
                    {
                        // 敵の forward を基準に回転
                        Quaternion rot = Quaternion.Euler(0, angle, 0) * transform.rotation;
                        Vector3 bulletMoveDirection = rot * Vector3.forward; // forward方向を回転させる

                        GameObject bullet = Instantiate(
                            bulletPrefab,
                            this.transform.position + new Vector3(0, offsetY, 0),
                            Quaternion.LookRotation(bulletMoveDirection) // 弾の向きを設定
                        );

                        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                        if (bulletRb != null)
                        {
                            bulletRb.velocity = bulletMoveDirection * bulletSpeed;
                        }

                        angle += angleStep;
                    }

                    state = State.Follow; // 攻撃後はFollow状態に戻る
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

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (state != State.Attack) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            transform.DOKill();
            rb.isKinematic = false;
            state = State.Charge;
        }
    }

    private void OnTriggerStay(Collider other)
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
        rb.isKinematic = false;

        state = State.Follow;
        attackIntervalCount = 0f;

        animator.ResetTrigger("isAttack");
        animator.ResetTrigger("isDead");
        animator.Play("Idle", 0, 0f);
    }

    public override void Damage(int _damage)
    {
        base.Damage(_damage);
        if (hp <= 0)
        {
            SoundManager.Instance.PlaySE("BossRainyDead");
        }
    }
}
