using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBossController : EnemyBase
{
    [SerializeField] float attackInterval = 5f;
    [SerializeField] float dashDistance = 5f;
    [SerializeField] float dashDuration = 1.5f;

    float attackIntervalCount = 0f;
    enum State { Follow, Charge, Attack }
    State state = State.Follow;

    LineRenderer lineRenderer;

    Tween dashTween; // クラスのメンバに保持する

    private void Start()
    {
        // LineRenderer 初期化
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void FixedUpdate()
    {
        if (!isActive || isDead) return;
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != gameManager.selectPlayer && gameManager.selectPlayer != null)
        {
            player = gameManager.selectPlayer;
            playerController = player.GetComponent<PlayerController>();
        }

        if (player != null && playerController != null)
        {
            Stan();
            if (playerController.GetIsSkill())
            {
                dashTween.Pause();
                return;
            }
            else
            {
                if (state == State.Charge)
                {
                    attackIntervalCount += Time.deltaTime;

                    // 線を時間に応じて伸ばす
                    Vector3 dashDirection = transform.forward.normalized;
                    dashDirection.y = 0;

                    Vector3 startPosition = transform.position;
                    Vector3 dashTarget = startPosition + dashDirection * dashDistance;
                    float t = Mathf.Clamp01(attackIntervalCount / attackInterval);
                    Vector3 currentEnd = Vector3.Lerp(startPosition, dashTarget, t);

                    lineRenderer.SetPosition(0, new Vector3(startPosition.x, 0, startPosition.z));
                    lineRenderer.SetPosition(1, new Vector3(currentEnd.x, 0, currentEnd.z));

                    if (attackIntervalCount > attackInterval)
                    {
                        attackIntervalCount = 0;
                        state = State.Attack;
                        animator.SetBool("isWait", false);

                        lineRenderer.enabled = false;

                        // Rigidbodyとの干渉を防ぐために一時的にKinematic
                        rb.isKinematic = true;

                        dashTween = transform.DOMove(dashTarget, dashDuration)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                rb.isKinematic = false;
                                state = State.Follow;
                                dashTween = null;
                            });
                    }
                }

                if (state == State.Follow)
                {
                    Vector3 direction = (player.transform.position - transform.position).normalized;
                    rb.velocity = direction * moveSpeed;
                    transform.LookAt(player.transform.position);
                }

                if(state == State.Attack && dashTween!= null) {
                    dashTween.Play();
                }
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.gameObject.CompareTag("Player"))
        {
            transform.DOKill(); // DOTween 移動停止
            rb.isKinematic = false;
            state = State.Charge;
            animator.SetBool("isWait", false);

            PrepareDashLine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.Follow) return;

        if (other.gameObject.CompareTag("Player"))
        {
            PrepareDashLine();
            animator.SetBool("isWait", true);
            state = State.Charge;
        }
    }

    void PrepareDashLine()
    {
        // 線を初期化して有効化
        Vector3 dashDirection = transform.forward.normalized;
        dashDirection.y = 0;

        Vector3 startPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 dashTarget = startPosition + dashDirection * dashDistance;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition); // 最初は0距離
        lineRenderer.enabled = true;

        attackIntervalCount = 0f;
    }

    public override void Hidden()
    {
        base.Hidden();
        dashTween = null;
        state = State.Follow;
        rb.WakeUp();
        rb.isKinematic = false;
        animator.SetBool("isWait", false);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    public override void Damage(int _damage)
    {
        base.Damage(_damage);

        if(hp <= 0)
        {
            Dead();
        }
    }
}
