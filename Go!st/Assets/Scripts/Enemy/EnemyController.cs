using UnityEngine;

public class EnemyController : EnemyBase
{
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] private bool isShot = false;
    [SerializeField, Header("弾の発射位置の高さ調整")] float offsetY = 0.5f;

    [SerializeField] private float shotCoolTime = 5f;
    [SerializeField] private float bulletSpeed = 10f; // 追加：弾の速度

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
        // bulletPrefab を自身の前方に生成
        GameObject bullet = Instantiate(bulletPrefab, transform.position + new Vector3(0, offsetY, 0), transform.rotation);

        // Rigidbody を取得して前方へ力を加える
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = transform.forward * bulletSpeed;
        }

        // 一定時間後に削除（例：5秒）
        Destroy(bullet, 5f);
    }
}
