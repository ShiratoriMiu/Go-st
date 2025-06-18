using UnityEngine;

public class EnemyController : EnemyBase
{
    void FixedUpdate()
    {
        if (!isActive || isDead) return;
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != gameManager.selectPlayer && gameManager.selectPlayer != null)
        {
            player = gameManager.selectPlayer;
            playerController = player.GetComponent<PlayerController>();
        }

        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if (player != null && playerController != null)
        {
            if (!playerController.GetIsSkill())
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;
                transform.LookAt(player.transform.position);
            }

            Stan();
        }
    }
}
