using UnityEngine;

public class EnemyController : EnemyBase
{
    void FixedUpdate()
    {
        if (!isActive) return;
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != gameManager.selectPlayer && gameManager.selectPlayer != null)
        {
            player = gameManager.selectPlayer;
            playerController = player.GetComponent<PlayerController>();
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
