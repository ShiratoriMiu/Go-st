using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotBossBulletController : MonoBehaviour
{
    [SerializeField] float destroyTime = 3f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Damage(1);
            Destroy(gameObject);
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            other.GetComponent<PlayerBulletController>().Hidden();
            Destroy(gameObject);
        }
    }
}
