using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBossController : EnemyBase
{
    private float attackIntervalCount = 0;

    [SerializeField] private float attackInterval = 5;
    [SerializeField] private float attackPower = 5;

    enum State
    {
        Wait,
        Attack,
    }
    State state = State.Wait;

    void FixedUpdate()
    {
        if (!isActive) return;

        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != gameManager.selectPlayer && gameManager.selectPlayer != null)
        {
            player = gameManager.selectPlayer;
            playerController = player.GetComponent<PlayerController>();
        }

        if (player != null)
        {
            if (!playerController.GetIsSkill())
            {
                if (state == State.Wait)
                {
                    // プレイヤーの方向を向く
                    transform.LookAt(player.transform.position);

                    attackIntervalCount += Time.deltaTime;
                    if (attackIntervalCount > attackInterval)
                    {
                        state = State.Attack;
                        attackIntervalCount = 0;
                    }
                }
                if (state == State.Attack)
                {
                    //突進
                    rb.AddForce(Vector3.forward * attackPower, ForceMode.Impulse);

                    //止まったら
                    if (rb.velocity == Vector3.zero)
                    {
                        state = State.Wait;
                    }
                }

                // 常に一定の速度で移動させる
                Vector3 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;


            }

            Stan();
        }
    }
}
