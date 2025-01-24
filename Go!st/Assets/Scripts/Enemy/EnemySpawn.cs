using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField,Header("�G�̃v���n�u")] GameObject enemyPrefab; // �G�̃v���n�u
    [SerializeField, Header("�G�̍ő吔")] private int maxPoolSize = 100; // �v�[���̍ő�T�C�Y
    [SerializeField, Header("���ɃX�|�[������܂ł̎���")] private float spawnTime = 10f;
    //�v���C���[����Ɏ���ɃX�|�[��
    [SerializeField, Header("�ŏ��X�|�[���ʒu")] private float spawnMinDistance = 5f;
    [SerializeField, Header("�ő�X�|�[���ʒu")] private float spawnMaxDistance = 10f;

    private GameObject player;
    private float time = 0;
    private PlayerController playerController;

    private List<GameObject> enemyPool; // �G�I�u�W�F�N�g�̃v�[��

    void Start()
    {
        // �v���C���[���擾
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        // �v�[����������
        enemyPool = new List<GameObject>();
        for (int i = 0; i < maxPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab,this.transform);
            enemy.SetActive(false); // ������ԂŔ�A�N�e�B�u�ɂ���
            enemyPool.Add(enemy);
        }
    }

    void Update()
    {
        if (playerController.GetIsSkill()) return;

        time += Time.deltaTime;

        if (time > spawnTime)
        {
            Spawn();
            time = 0;
        }
    }

    void Spawn()
    {
        // �����_���ȕ����𐶐� (XZ����)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // �����_���ȋ����𐶐�
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);

        // �I�t�Z�b�g���v�Z
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0, // �����͕K�v�ɉ����Ē���
            randomDirection.y * randomDistance
        );

        // �v���C���[�̈ʒu����ɔz�u
        spawnPosition += player.transform.position;

        // �v�[�������A�N�e�B�u�ȓG���擾
        GameObject enemy = GetPooledEnemy();
        if (enemy != null)
        {
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = Quaternion.identity;
            enemy.SetActive(true); // �G���A�N�e�B�u�ɂ���
        }
    }

    // �v�[�������A�N�e�B�u�ȓG���擾
    GameObject GetPooledEnemy()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy) // ��A�N�e�B�u�ȃI�u�W�F�N�g��T��
            {
                return enemy;
            }
        }
        return null; // �v�[�������ׂĎg�p���̏ꍇ�� null ��Ԃ�
    }
}
