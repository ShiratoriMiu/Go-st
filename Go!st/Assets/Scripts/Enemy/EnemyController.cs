using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    GameObject player;
    Rigidbody rb;
    PlayerController playerController;
    LevelManager levelManager;
    GameManager gameManager;
    Vector3 lastVelocity;
    private float hp;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int attack = 1;

    [SerializeField] Animator animator;
    [SerializeField] private Renderer[] enemyRenderers;
    [SerializeField] private Collider enemyCollider;

    private bool previousSkillState = false; // 前回のスキル状態を記録

    public bool isActive { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hp = maxHp;
    }

    public void Initialize(GameManager _gameManager, LevelManager _levelManager)
    {
        gameManager = _gameManager;
        levelManager = _levelManager;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isActive) return;

        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != gameManager.selectPlayer && gameManager.selectPlayer != null)
        {
            player = gameManager.selectPlayer;
            playerController = player.GetComponent<PlayerController>();
        }

        if (player != null)
        {
            if (!playerController.GetIsSkill())
            {
                // 常に一定の速度で移動させる
                Vector3 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;

                // プレイヤーの方向を向く
                transform.LookAt(player.transform.position);
            }

            Stan();
        }
    }

    void Stan()
    {
        bool currentSkillState = playerController.GetIsSkill();

        // スキル状態が true に変化したタイミングで1回だけ実行
        if (!previousSkillState && currentSkillState)
        {
            // 停止時の速度を記録し、Rigidbody を停止
            lastVelocity = rb.velocity;
            rb.Sleep();

            // アニメーションを停止
            animator.speed = 0f;
        }

        // スキル状態が false に戻った場合の処理
        if (previousSkillState && !currentSkillState)
        {
            // Rigidbody を再開し、速度を復元
            rb.WakeUp();
            rb.velocity = lastVelocity;

            // アニメーションを再開
            animator.speed = 1f;
        }

        // スキル状態を更新
        previousSkillState = currentSkillState;
    }

    public void Damage(int _damage)
    {
        hp -= _damage;
        Dead();
    }

    void Dead()
    {
        if (hp <= 0)
        {
            Hidden();
            hp = maxHp;
            levelManager.AddEnemyKill();
            gameManager.AddEnemiesDefeatedNum();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!playerController.GetIsSkill())
            {
                collision.gameObject.GetComponent<PlayerController>().Damage(attack);
            }
        }
    }

    public void Display()
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

    public void Hidden()
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
