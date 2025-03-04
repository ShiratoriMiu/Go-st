using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        public GameObject enemyPrefab; // �G�̃v���n�u
        public int level; // ���̓G�̃��x��
    }

    [SerializeField, Header("�G�f�[�^���X�g")]
    private List<EnemyData> enemyDataList; // �G�f�[�^�̃��X�g

    [SerializeField, Header("�G�̍ő吔")]
    private int maxPoolSize = 100; // �v�[���̍ő�T�C�Y

    [SerializeField, Header("���ɃX�|�[������܂ł̎���")]
    private float spawnTime = 10f;

    [SerializeField, Header("�ŏ��X�|�[���ʒu")]
    private float spawnMinDistance = 5f;

    [SerializeField, Header("�ő�X�|�[���ʒu")]
    private float spawnMaxDistance = 10f;

    private GameObject player;
    private float time = 0;
    private PlayerController playerController;
    private LevelManager levelManager;
    private GameManager gameManager;

    private Dictionary<GameObject, List<GameObject>> enemyPools; // �G���Ƃ̃I�u�W�F�N�g�v�[��

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        // �v�[����������
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        foreach (var enemyData in enemyDataList)
        {
            enemyPools[enemyData.enemyPrefab] = new List<GameObject>();
            for (int i = 0; i < maxPoolSize / enemyDataList.Count; i++) // �e�G���Ƃɋϓ��Ƀv�[��
            {
                GameObject enemy = Instantiate(enemyData.enemyPrefab, this.transform);
                enemy.SetActive(false);
                enemyPools[enemyData.enemyPrefab].Add(enemy);
            }
        }
    }

    void Update()
    {
        if (gameManager.state == GameManager.GameState.Title)
        {
            DeactivateAllEnemies(); // ���ׂĂ̓G���A�N�e�B�u�ɂ���
            return;
        }

        if (gameManager.state != GameManager.GameState.Game) return;
        if (playerController.GetIsSkill()) return;

        time += Time.deltaTime;

        if (time > spawnTime)
        {
            Spawn();
            time = 0;
        }
    }

    // ���ׂĂ̓G���A�N�e�B�u�ɂ���
    void DeactivateAllEnemies()
    {
        foreach (var pool in enemyPools.Values)
        {
            foreach (var enemy in pool)
            {
                enemy.SetActive(false);
            }
        }
    }

    void Spawn()
    {
        int currentLevel = levelManager.GetCurrentLevel(); // ���݂̃��x�����擾

        // ���x�������𖞂����G�����X�g�A�b�v
        List<EnemyData> validEnemies = enemyDataList.FindAll(e => e.level <= currentLevel);
        if (validEnemies.Count == 0) return; // �X�|�[���ł���G�����Ȃ��ꍇ�͏I��

        // �����_���ɓG��I��
        EnemyData selectedEnemyData = validEnemies[Random.Range(0, validEnemies.Count)];

        // �X�|�[���ʒu���v�Z
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0,
            randomDirection.y * randomDistance
        ) + player.transform.position;

        // �v�[������擾
        GameObject enemy = GetPooledEnemy(selectedEnemyData.enemyPrefab);
        if (enemy != null)
        {
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = Quaternion.identity;
            enemy.SetActive(true);
        }
    }

    GameObject GetPooledEnemy(GameObject prefab)
    {
        foreach (var enemy in enemyPools[prefab])
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null;
    }
}
