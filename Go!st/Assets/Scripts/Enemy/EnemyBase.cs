using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public event System.Action OnDeath;

    protected GameObject player;
    protected Rigidbody rb;
    protected PlayerController playerController;
    protected LevelManager levelManager;
    protected GameManager gameManager;
    protected Vector3 lastVelocity;
    protected float hp;

    [SerializeField] protected float moveSpeed = 0;
    [SerializeField] protected int maxHp = 10;
    [SerializeField] protected int attack = 1;
    [SerializeField] protected int defeatedNum = 1;


[SerializeField] protected Animator animator;
    [SerializeField] protected Renderer[] enemyRenderers;
    [SerializeField] protected Collider enemyCollider;

    protected bool previousSkillState = false;

    public bool isActive { get; protected set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hp = maxHp;
    }

    public virtual void Initialize(GameManager _gameManager, LevelManager _levelManager)
    {
        gameManager = _gameManager;
        levelManager = _levelManager;
    }

    protected virtual void Stan()
    {
        if (playerController == null) return;

        bool currentSkillState = playerController.GetIsSkill();

        if (!previousSkillState && currentSkillState)
        {
            lastVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
            rb.Sleep();
            animator.speed = 0f;
        }

        if (previousSkillState && !currentSkillState)
        {
            rb.WakeUp();
            rb.velocity = lastVelocity;
            animator.speed = 1f;
        }

        previousSkillState = currentSkillState;
    }

    public virtual void Damage(int _damage)
    {
        hp -= _damage;
        Dead();
    }

    protected virtual void Dead()
    {
        if (hp <= 0)
        {
            Hidden();
            hp = maxHp;
            levelManager?.AddEnemyKill();
            gameManager?.AddEnemiesDefeatedNum(defeatedNum);

            OnDeath?.Invoke(); // ƒ{ƒX/ŽG‹›‹¤’Ê‚Å”­‰Î
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerController != null && !playerController.GetIsSkill())
            {
                collision.gameObject.GetComponent<PlayerController>().Damage(attack);
            }
        }
    }

    public virtual void Display()
    {
        isActive = true;
        animator.enabled = true;
        enemyCollider.enabled = true;
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.enabled = true;
        }
        rb.useGravity = true;
    }

    public virtual void Hidden()
    {
        isActive = false;
        animator.enabled = false;
        enemyCollider.enabled = false;
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.enabled = false;
        }
        rb.useGravity = false;
    }
}
