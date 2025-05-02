using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField, Header("���ׂĂ̓G�v���n�u���")]
    private List<GameObject> allEnemyPrefabs; // �g�p�\�Ȃ��ׂĂ̓G�v���n�u

    [SerializeField, Header("�G�̍ő吔")]
    private int maxPoolSize = 100;

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

    private Dictionary<GameObject, List<GameObject>> enemyPools;
    private List<GameObject> currentEnemyPrefabs = new List<GameObject>(); // ���݂̃��x���Ŏg���G�v���n�u

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        enemyPools = new Dictionary<GameObject, List<GameObject>>();

        // ���ׂẴv���n�u�Ńv�[��������
        foreach (var prefab in allEnemyPrefabs)
        {
            enemyPools[prefab] = new List<GameObject>();
            for (int i = 0; i < maxPoolSize / allEnemyPrefabs.Count; i++)
            {
                GameObject enemy = Instantiate(prefab, this.transform);
                enemy.SetActive(false);
                enemyPools[prefab].Add(enemy);
            }
        }
    }

    void Update()
    {
        if (gameManager.state == GameManager.GameState.Title)
        {
            DeactivateAllEnemies();
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
        if (currentEnemyPrefabs.Count == 0) return;

        // �����_���ɓG��I��
        GameObject selectedPrefab = currentEnemyPrefabs[Random.Range(0, currentEnemyPrefabs.Count)];

        // �X�|�[���ʒu������
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0,
            randomDirection.y * randomDistance
        ) + player.transform.position;

        GameObject enemy = GetPooledEnemy(selectedPrefab);
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

    public void SetEnemyTypesByLevelData(LevelData levelData)
    {
        currentEnemyPrefabs.Clear();

        // CSV�̕�������X���b�V���ŕ������A�X�̓G���ɂ���
        List<string> enemyNames = levelData.EnemyTypes;

        foreach (var enemyName in enemyNames)
        {
            //�󂯎�����G�̖��O��/���Ƃɕ���
            string[] splitName = enemyName.Split('/');

            // �v���n�u������v������̂������i�啶���������𖳎��j
            for (int i = 0; i < splitName.Length; i++)
            {
                GameObject prefab = allEnemyPrefabs.Find(p => p.name.Equals(splitName[i], System.StringComparison.OrdinalIgnoreCase));
                if (prefab != null)
                {
                    currentEnemyPrefabs.Add(prefab);
                    Debug.Log($"Enemy prefab added: {prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"�G�v���n�u��������܂���: {splitName[i]}");
                }
            }
        }
    }

}
