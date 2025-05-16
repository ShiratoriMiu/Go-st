using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DushTest : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] float attackInterval = 5f;
    [SerializeField] float dashDistance = 5f;
    [SerializeField] float dashDuration = 1.5f;

    float attackIntervalCount = 0f;
    Animator animator;
    enum State { Follow, Charge, Attack }
    State state = State.Follow;

    Rigidbody rb;
    LineRenderer lineRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // LineRenderer ‰Šú‰»
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (state == State.Charge)
        {
            attackIntervalCount += Time.deltaTime;

            // ü‚ğŠÔ‚É‰‚¶‚ÄL‚Î‚·
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
                animator.SetTrigger("isAttack");

                lineRenderer.enabled = false;

                // Rigidbody‚Æ‚ÌŠ±Â‚ğ–h‚®‚½‚ß‚Éˆê“I‚ÉKinematic
                rb.isKinematic = true;

                transform.DOMove(dashTarget, dashDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        rb.isKinematic = false;
                        state = State.Follow;
                        animator.SetTrigger("isWait");
                    });
            }
        }

        if (state == State.Follow)
        {
            if (!player.GetIsSkill())
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * 3f;
                transform.LookAt(player.transform.position);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Attack) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            transform.DOKill(); // DOTween ˆÚ“®’â~
            rb.isKinematic = false;
            state = State.Charge;
            animator.SetTrigger("isWait");

            PrepareDashLine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.Follow) return;

        if (other.gameObject.CompareTag("Player"))
        {
            PrepareDashLine();
            state = State.Charge;
        }
    }

    void PrepareDashLine()
    {
        // ü‚ğ‰Šú‰»‚µ‚Ä—LŒø‰»
        Vector3 dashDirection = transform.forward.normalized;
        dashDirection.y = 0;

        Vector3 startPosition = new Vector3(transform.position.x,0, transform.position.z);
        Vector3 dashTarget = startPosition + dashDirection * dashDistance;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition); // Å‰‚Í0‹——£
        lineRenderer.enabled = true;

        attackIntervalCount = 0f;
    }
}
