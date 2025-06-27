using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class EnemyConfig
{
    public GameObject prefab;
    public int maxCount;
}

public class EnemyManager : MonoBehaviour
{
    [SerializeField, Header("すべての敵プレハブと最大数")]
    private List<EnemyConfig> enemyConfigs;

    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private float spawnTime = 10f;
    [SerializeField] private float spawnMinDistance = 5f;
    [SerializeField] private float spawnMaxDistance = 10f;

    [SerializeField] private PlayerManager playerManager;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;

    private GameObject player;
    private PlayerController playerController;
    private float timer = 0f;
    private Dictionary<GameObject, List<GameObject>> enemyPools = new();
    private Dictionary<GameObject, int> enemyMaxCounts = new();
    private List<GameObject> currentEnemyPrefabs = new();

    private bool isSpawningEnabled = false;

    [Header("ボス関連設定")]
    [SerializeField] private List<GameObject> bossPrefabs;

    private Dictionary<GameObject, List<GameObject>> bossPools = new();
    private Dictionary<GameObject, int> bossMaxCounts = new();
    [SerializeField] private int bossMaxCountPerPrefab = 1; // ボスは基本1体ずつ

    private int nextBossThreshold = 10;
    private GameObject currentBossInstance = null;
    private bool isBossActive = false;

    void Update()
    {
        if (!isSpawningEnabled) return;
        if (gameManager.state != GameManager.GameState.Game) return;
        if (playerController.GetIsSkill()) return;

        // 通常の敵スポーン
        timer += Time.deltaTime;
        if (timer >= spawnTime)
        {
            SpawnEnemy();
            timer = 0f;
        }

        // ボス出現条件をチェック
        CheckAndSpawnBoss();
    }

    public void InitializePools()
    {
        // 敵プール初期化
        enemyPools.Clear();
        enemyMaxCounts.Clear();

        foreach (var config in enemyConfigs)
        {
            enemyPools[config.prefab] = new List<GameObject>();
            enemyMaxCounts[config.prefab] = config.maxCount;

            for (int i = 0; i < config.maxCount; i++)
            {
                GameObject enemy = Instantiate(config.prefab, this.transform);
                var controller = enemy.GetComponent<EnemyController>();
                controller.Initialize(gameManager, levelManager, player, playerController);
                controller.Hidden();
                enemyPools[config.prefab].Add(enemy);
            }
        }

        // ボスプール初期化
        bossPools.Clear();
        bossMaxCounts.Clear();

        foreach (var bossPrefab in bossPrefabs)
        {
            bossPools[bossPrefab] = new List<GameObject>();
            bossMaxCounts[bossPrefab] = bossMaxCountPerPrefab;

            for (int i = 0; i < bossMaxCountPerPrefab; i++)
            {
                GameObject boss = Instantiate(bossPrefab, this.transform);
                var controller = boss.GetComponent<EnemyBase>();
                controller.Initialize(gameManager, levelManager, player, playerController);
                controller.Hidden();

                controller.OnDeath -= OnBossDefeated;
                controller.OnDeath += OnBossDefeated;

                bossPools[bossPrefab].Add(boss);
            }
        }
    }

    public void SetEnemyTypesByLevelData(LevelData levelData)
    {
        currentEnemyPrefabs.Clear();

        foreach (var enemyName in levelData.EnemyTypes)
        {
            string[] enemyTypes = enemyName.Split('/');
            for (int i = 0; i < enemyTypes.Length; i++)
            {
                var config = enemyConfigs.Find(c => c.prefab.name == enemyTypes[i]);
                if (config != null)
                {
                    currentEnemyPrefabs.Add(config.prefab);
                }
                else
                {
                    Debug.LogWarning($"敵プレハブが見つかりません: {enemyTypes[i]}");
                }
            }
        }
    }

    void SpawnEnemy()
    {
        if (currentEnemyPrefabs.Count == 0) return;

        GameObject selectedPrefab = currentEnemyPrefabs[Random.Range(0, currentEnemyPrefabs.Count)];

        Vector2 direction = Random.insideUnitCircle.normalized;
        float distance = Random.Range(spawnMinDistance, spawnMaxDistance);
        Vector3 spawnPos = player.transform.position + new Vector3(direction.x, 0, direction.y) * distance;

        GameObject enemy = GetPooledEnemy(selectedPrefab);
        if (enemy != null)
        {
            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;
            var controller = enemy.GetComponent<EnemyController>();
            controller.Display();
        }
    }

    GameObject GetPooledEnemy(GameObject prefab)
    {
        foreach (var enemy in enemyPools[prefab])
        {
            var controller = enemy.GetComponent<EnemyController>();
            if (!controller.isActive)
                return enemy;
        }
        return null;
    }

    GameObject GetPooledBoss(GameObject prefab)
    {
        foreach (var boss in bossPools[prefab])
        {
            var controller = boss.GetComponent<EnemyBase>();
            if (!controller.isActive)
                return boss;
        }
        return null;
    }

    public void StartSpawning()
    {
        isSpawningEnabled = true;
        timer = 0f;
        player = playerManager.Player;
        playerController = player.GetComponent<PlayerController>();

        InitializePools();
    }

    public void StopSpawning()
    {
        isSpawningEnabled = false;
    }

    public void ResetEnemies()
    {
        StopSpawning();

        // 敵非表示
        foreach (var pool in enemyPools.Values)
        {
            foreach (var enemy in pool)
            {
                var controller = enemy.GetComponent<EnemyController>();
                controller.Hidden();
            }
        }

        // ボス非表示
        foreach (var pool in bossPools.Values)
        {
            foreach (var boss in pool)
            {
                var controller = boss.GetComponent<EnemyBase>();
                controller.Hidden();
            }
        }

        currentBossInstance = null;
        isBossActive = false;
    }

    void CheckAndSpawnBoss()
    {
        if (gameManager.enemiesDefeated >= nextBossThreshold && !isBossActive)
        {
            SpawnBoss();
            nextBossThreshold += 10;
        }
    }

    void SpawnBoss()
    {
        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count - 1)];

        GameObject boss = GetPooledBoss(bossPrefab);
        if (boss == null)
        {
            Debug.LogWarning("利用可能なボスがいません");
            return;
        }

        Vector2 direction = Random.insideUnitCircle.normalized;
        float distance = Random.Range(spawnMinDistance, spawnMaxDistance);
        Vector3 spawnPos = player.transform.position + new Vector3(direction.x, 0, direction.y) * distance;

        boss.transform.position = spawnPos;
        boss.transform.rotation = Quaternion.identity;

        var controller = boss.GetComponent<EnemyBase>();
        controller.Display();

        currentBossInstance = boss;
        isBossActive = true;
    }

    void OnBossDefeated()
    {
        Debug.Log("ボス倒れた");
        isBossActive = false;
        currentBossInstance = null;

        if (gameManager.enemiesDefeated >= nextBossThreshold)
        {
            SpawnBoss();
            nextBossThreshold += 10;
        }
    }
}
