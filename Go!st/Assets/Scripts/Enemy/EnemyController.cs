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

    private bool previousSkillState = false; // �O��̃X�L����Ԃ��L�^

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        hp = maxHp;
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameManager.state != GameManager.GameState.Game) return;

        if (player != null)
        {
            if (!playerController.GetIsSkill())
            {
                // ��Ɉ��̑��x�ňړ�������
                Vector3 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;

                // �v���C���[�̕���������
                transform.LookAt(player.transform.position);
            }

            Stan();
        }
    }

    void Stan()
    {
        bool currentSkillState = playerController.GetIsSkill();

        // �X�L����Ԃ� true �ɕω������^�C�~���O��1�񂾂����s
        if (!previousSkillState && currentSkillState)
        {
            // ��~���̑��x���L�^���ARigidbody ���~
            lastVelocity = rb.velocity;
            rb.Sleep();

            // �A�j���[�V�������~
            animator.speed = 0f;

            Debug.Log("Stan ������1����s���܂���");
        }

        // �X�L����Ԃ� false �ɖ߂����ꍇ�̏���
        if (previousSkillState && !currentSkillState)
        {
            // Rigidbody ���ĊJ���A���x�𕜌�
            rb.WakeUp();
            rb.velocity = lastVelocity;

            // �A�j���[�V�������ĊJ
            animator.speed = 1f;

            Debug.Log("Stan ��ԉ������������s���܂���");
        }

        // �X�L����Ԃ��X�V
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
            this.gameObject.SetActive(false);
            hp = maxHp;
            levelManager.AddEnemyNum();
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
}
