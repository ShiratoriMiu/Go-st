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
    protected PlayerSkill playerSkill;
    protected EnemyDeadEffectManager enemyDeadEffectManager;
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
    [SerializeField] protected Collider[] enemyColliders;

    protected bool previousSkillState = false;

    public bool isActive { get; protected set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hp = maxHp;
    }

    public virtual void Initialize(GameManager _gameManager, LevelManager _levelManager,
        GameObject _player, PlayerController _playerController, EnemyDeadEffectManager _enemyDeadEffectManager)
    {
        gameManager = _gameManager;
        levelManager = _levelManager;
        player = _player;
        playerController = _playerController;
        playerSkill = _player.GetComponent<PlayerSkill>();
        enemyDeadEffectManager = _enemyDeadEffectManager;
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
            if(!playerSkill.GetIsSkill()) SoundManager.Instance.PlaySE("HitSEFinish", 0.3f);
            animator.speed = 1f;
            isDead = true;
            animator.SetTrigger("isDead");
            OnDeath?.Invoke(); // ボス/雑魚共通で発火
            Debug.Log("ボス死亡発火");
            // 複数Colliderを無効化
            foreach (var col in enemyColliders)
            {
                col.enabled = false;
            }
            rb.useGravity = false;
        }
        else
        {
            if (!playerSkill.GetIsSkill()) SoundManager.Instance.PlaySE("HitSE", 0.3f);
        }
    }

    private IEnumerator StopMoveCoroutine()
    {
        canMove = false;       // 移動禁止にする
        rb.velocity = Vector3.zero;  // 念のためゼロに

        yield return new WaitForSeconds(0.5f);  // 0.5秒停止

        canMove = true;        // 移動再開
    }

    //死亡アニメーションのアニメーションイベントで呼ぶ
    public virtual void Dead()
    {
        SoundManager.Instance.PlaySE("DeadSE");
        hp = maxHp;
        levelManager?.AddEnemyKill();
        gameManager?.AddEnemiesDefeatedNum(defeatedNum);
        enemyDeadEffectManager.PlayEffect(this.transform.position);

        Hidden();
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
        // 複数Colliderを無効化
        foreach (var col in enemyColliders)
        {
            col.enabled = true;
        }
        //アニメーターのトリガーをリセット
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(p.name);
        }


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
        // 複数Colliderを無効化
        foreach (var col in enemyColliders)
        {
            col.enabled = false;
        }
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.enabled = false;
        }
        rb.useGravity = false;
        isDead = false;
    }
}
