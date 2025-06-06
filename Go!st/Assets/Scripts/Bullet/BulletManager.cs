using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 20;

    private List<GameObject> bulletPool = new List<GameObject>();

    GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        PlayerBulletController playerBulletController = bullet.GetComponent<PlayerBulletController>();
        playerBulletController.Hidden();
        bulletPool.Add(bullet);
        return bullet;
    }

    public GameObject GetBullet()
    {
        // 空いてる弾を探す
        foreach (var bullet in bulletPool)
        {
            PlayerBulletController playerBulletController = bullet.GetComponent<PlayerBulletController>();
            if (!playerBulletController.isActive)
            {
                return bullet;
            }
        }

        // 空きがなければ新しく作る
        return CreateNewBullet();
    }
}
