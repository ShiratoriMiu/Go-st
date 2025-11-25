using UnityEngine;

public class ShotBossBulletController : MonoBehaviour
{
    [SerializeField] float destroyTime = 3f;

    private Rigidbody rb;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;
    private bool isPaused = false;

    private float timer = 0f;   // Destroy 用の手動タイマー

    PlayerSkill playerSkill;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Player をタグで検索
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerSkill = playerObj.GetComponent<PlayerSkill>();
        }
    }

    private void Update()
    {
        if (playerSkill && playerSkill.GetIsSkill())
        {
            Pause();
        }
        else
        {
            Resume();
        }

        // ---- Destroyタイマー ----
        if (!isPaused)
        {
            timer += Time.deltaTime;
            if (timer >= destroyTime)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Pause()
    {
        if (isPaused || rb == null) return;
        isPaused = true;

        savedVelocity = rb.velocity;
        savedAngularVelocity = rb.angularVelocity;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
    }

    public void Resume()
    {
        if (!isPaused || rb == null) return;
        isPaused = false;

        rb.isKinematic = false;

        rb.velocity = savedVelocity;
        rb.angularVelocity = savedAngularVelocity;
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
