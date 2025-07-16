using UnityEngine;

public class EnemyController : EnemyBase
{
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] private bool isShot = false;

    [SerializeField] private float shotCoolTime = 5f;
    [SerializeField] private float bulletSpeed = 10f; // �ǉ��F�e�̑��x

    private float currentTime = 0;

    void FixedUpdate()
    {
        if (!isActive || isDead) return;
        if (gameManager.state != GameManager.GameState.Game) return;

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

                if (isShot)
                {
                    currentTime += Time.deltaTime;
                    if(currentTime > shotCoolTime)
                    {
                        Shot();
                        currentTime = 0;
                    }
                }
            }

            Stan();
        }
    }

    void Shot()
    {
        // bulletPrefab �����g�̑O���ɐ���
        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation);

        // Rigidbody ���擾���đO���֗͂�������
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = transform.forward * bulletSpeed;
        }

        // ��莞�Ԍ�ɍ폜�i��F5�b�j
        Destroy(bullet, 5f);
    }
}
