using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public event System.Action OnDeath;

    protected GameObject player;
    protected Rigidbody rb;
    protected PlayerController playerController;
    protected LevelManager levelManager;
    protected GameManager gameManager;
    protected PlayerManager playerManager;
    protected Vector3 lastVelocity;
    protected float hp;
    protected bool canMove = true;
    protected bool isDead = false;

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

    public virtual void Initialize(GameManager _gameManager, LevelManager _levelManager, GameObject _player, PlayerController _playerController)
    {
        gameManager = _gameManager;
        levelManager = _levelManager;
        player = _player;
        playerController = _playerController;
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
        if (!isActive || isDead) return;
        animator.SetTrigger("isHit");
        StartCoroutine(StopMoveCoroutine());
        hp -= _damage;
        if (hp <= 0)
        {
            animator.speed = 1f;
            isDead = true;
            animator.SetTrigger("isDead");
            OnDeath?.Invoke(); // ƒ{ƒX/ŽG‹›‹¤’Ê‚Å”­‰Î
            enemyCollider.enabled = false;
            rb.useGravity = false;
        }
    }

    private IEnumerator StopMoveCoroutine()
    {
        canMove = false;       // ˆÚ“®‹ÖŽ~‚É‚·‚é
        rb.velocity = Vector3.zero;  // ”O‚Ì‚½‚ßƒ[ƒ‚É

        yield return new WaitForSeconds(0.5f);  // 0.5•b’âŽ~

        canMove = true;        // ˆÚ“®ÄŠJ
    }

    //Ž€–SƒAƒjƒ[ƒVƒ‡ƒ“‚ÌƒAƒjƒ[ƒVƒ‡ƒ“ƒCƒxƒ“ƒg‚ÅŒÄ‚Ô
    public virtual void Dead()
    {
        Hidden();
        hp = maxHp;
        levelManager?.AddEnemyKill();
        gameManager?.AddEnemiesDefeatedNum(defeatedNum);
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
        isDead = false;
    }
}
