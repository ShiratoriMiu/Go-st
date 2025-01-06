using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    GameObject player;
    Rigidbody rb;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private int hp = 10;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            transform.LookAt(player.transform.position);

            rb.AddForce(transform.forward * moveSpeed * Time.deltaTime, ForceMode.Force);
        }

        if (player.GetComponent<PlayerController>().GetIsSkill())
        {
            rb.Sleep();
        }
        else
        {
            rb.WakeUp();
        }

        Dead();
    }

    public void Damage(int _damage)
    {
        hp -= _damage;
    }

    void Dead()
    {
        if(hp <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SkillArea"))
        {
            Damage(3);
        }
    }
}
