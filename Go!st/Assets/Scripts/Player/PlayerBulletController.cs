using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    [SerializeField] float destroyTime = 3f;

    PlayerController playerController;

    [SerializeField] Collider bulletCollider;
    [SerializeField] Renderer bulletRenderer;

    public bool isActive { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyController>().Damage(1);
            Hidden();
        }
    }

    public void Display()
    {
        bulletCollider.enabled = true;
        bulletRenderer.enabled = true;
        isActive = true;

        CancelInvoke();
        Invoke("Hidden", destroyTime); // ”ñ•\Ž¦
    }

    public void Hidden()
    {
        bulletCollider.enabled = false;
        bulletRenderer.enabled = false;
        isActive = false;
    }
}
