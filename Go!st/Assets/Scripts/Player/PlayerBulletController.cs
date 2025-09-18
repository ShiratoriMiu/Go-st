using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    [SerializeField] float destroyTime = 3f;

    PlayerController playerController;

    [SerializeField] Collider bulletCollider;
    [SerializeField] GameObject bulletEffect;

    public bool isActive { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBase>().Damage(1);
            Hidden();
        }
    }

    public void Display()
    {
        bulletCollider.enabled = true;
        isActive = true;

        bulletEffect.SetActive(true);

        CancelInvoke();
        Invoke("Hidden", destroyTime); // ”ñ•\Ž¦
    }

    public void Hidden()
    {
        bulletEffect.SetActive(false);

        bulletCollider.enabled = false;
        isActive = false;
    }
}
