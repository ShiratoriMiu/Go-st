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
        // ‹ó‚¢‚Ä‚é’e‚ğ’T‚·
        foreach (var bullet in bulletPool)
        {
            PlayerBulletController playerBulletController = bullet.GetComponent<PlayerBulletController>();
            if (!playerBulletController.isActive)
            {
                return bullet;
            }
        }

        // ‹ó‚«‚ª‚È‚¯‚ê‚ÎV‚µ‚­ì‚é
        return CreateNewBullet();
    }
}
