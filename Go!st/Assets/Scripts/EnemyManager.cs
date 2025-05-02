using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyConfig
{
    public GameObject prefab;
    public int maxCount;
}


public class EnemyManager : MonoBehaviour
{
    [SerializeField, Header("Ç∑Ç◊ÇƒÇÃìGÉvÉåÉnÉuÇ∆ç≈ëÂêî")]
    private List<EnemyConfig> enemyConfigs;

    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private float spawnTime = 10f;
    [SerializeField] private float spawnMinDistance = 5f;
    [SerializeField] private float spawnMaxDistance = 10f;

    [SerializeField] private PlayerSelect playerSelect;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;

    private GameObject player;
    private PlayerController playerController;
    private float timer = 0f;
    private Dictionary<GameObject, List<GameObject>> enemyPools = new();
    private Dictionary<GameObject, int> enemyMaxCounts = new();
    private List<GameObject> currentEnemyPrefabs = new();

    private bool isSpawningEnabled = false;

    void Start()
    {
        InitializePools();
    }

    void Update()
    {
        if (!isSpawningEnabled) return;
        if (gameManager.state != GameManager.GameState.Game) return;
        if (playerController.GetIsSkill()) return;

        timer += Time.deltaTime;
        if (timer >= spawnTime)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    public void InitializePools()
    {
        enemyPools.Clear();
        enemyMaxCounts.Clear();

        foreach (var config in enemyConfigs)
        {
            enemyPools[config.prefab] = new List<GameObject>();
            enemyMaxCounts[config.prefab] = config.maxCount;
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
                // enemyConfigs Ç©ÇÁ name àÍívÇ∑ÇÈ prefab ÇíTÇ∑
                var config = enemyConfigs.Find(c => c.prefab.name == enemyTypes[i]);
                if (config != null)
                {
                    currentEnemyPrefabs.Add(config.prefab);
                }
                else
                {
                    Debug.LogWarning($"ìGÉvÉåÉnÉuÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ: {enemyTypes[i]}");
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
            if (!enemy.GetComponent<EnemyController>().isActive)
                return enemy;
        }

        // è„å¿ñ¢ñûÇ»ÇÁêVãKê∂ê¨
        if (enemyPools[prefab].Count < enemyMaxCounts[prefab])
        {
            GameObject newEnemy = Instantiate(prefab, this.transform);
            var controller = newEnemy.GetComponent<EnemyController>();
            controller.Initialize(gameManager, levelManager);
            controller.Hidden();
            enemyPools[prefab].Add(newEnemy);
            return newEnemy;
        }

        // è„å¿Ç…íBÇµÇƒÇ¢ÇΩÇÁnullÇï‘Ç∑
        return null;
    }

    public void StartSpawning()
    {
        isSpawningEnabled = true;
        timer = 0f;
        player = playerSelect.selectPlayer;
        playerController = player.GetComponent<PlayerController>();
    }

    public void StopSpawning()
    {
        isSpawningEnabled = false;
    }

    public void ResetEnemies()
    {
        StopSpawning();
        foreach (var pool in enemyPools.Values)
        {
            foreach (var enemy in pool)
            {
                var controller = enemy.GetComponent<EnemyController>();
                controller.Hidden();
            }
        }
    }
}
