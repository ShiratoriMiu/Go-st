using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField, Header("すべての敵プレハブ候補")]
    private List<GameObject> allEnemyPrefabs; // 使用可能なすべての敵プレハブ

    [SerializeField, Header("敵の最大数")]
    private int maxPoolSize = 100;

    [SerializeField, Header("次にスポーンするまでの時間")]
    private float spawnTime = 10f;

    [SerializeField, Header("最小スポーン位置")]
    private float spawnMinDistance = 5f;

    [SerializeField, Header("最大スポーン位置")]
    private float spawnMaxDistance = 10f;

    private GameObject player;
    private float time = 0;
    private PlayerController playerController;
    private LevelManager levelManager;
    private GameManager gameManager;

    private Dictionary<GameObject, List<GameObject>> enemyPools;
    private List<GameObject> currentEnemyPrefabs = new List<GameObject>(); // 現在のレベルで使う敵プレハブ

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        enemyPools = new Dictionary<GameObject, List<GameObject>>();

        // すべてのプレハブでプールを準備
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

        // ランダムに敵を選択
        GameObject selectedPrefab = currentEnemyPrefabs[Random.Range(0, currentEnemyPrefabs.Count)];

        // スポーン位置を決定
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

        // CSVの文字列をスラッシュで分割し、個々の敵名にする
        List<string> enemyNames = levelData.EnemyTypes;

        foreach (var enemyName in enemyNames)
        {
            //受け取った敵の名前を/ごとに分割
            string[] splitName = enemyName.Split('/');

            // プレハブ名が一致するものを検索（大文字小文字を無視）
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
                    Debug.LogWarning($"敵プレハブが見つかりません: {splitName[i]}");
                }
            }
        }
    }

}
