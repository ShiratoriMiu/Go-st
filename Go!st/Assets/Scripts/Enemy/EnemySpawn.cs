using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        public GameObject enemyPrefab; // 敵のプレハブ
        public int level; // その敵のレベル
    }

    [SerializeField, Header("敵データリスト")]
    private List<EnemyData> enemyDataList; // 敵データのリスト

    [SerializeField, Header("敵の最大数")]
    private int maxPoolSize = 100; // プールの最大サイズ

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

    private Dictionary<GameObject, List<GameObject>> enemyPools; // 敵ごとのオブジェクトプール

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        // プールを初期化
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        foreach (var enemyData in enemyDataList)
        {
            enemyPools[enemyData.enemyPrefab] = new List<GameObject>();
            for (int i = 0; i < maxPoolSize / enemyDataList.Count; i++) // 各敵ごとに均等にプール
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
            DeactivateAllEnemies(); // すべての敵を非アクティブにする
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

    // すべての敵を非アクティブにする
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
        int currentLevel = levelManager.GetCurrentLevel(); // 現在のレベルを取得

        // レベル条件を満たす敵をリストアップ
        List<EnemyData> validEnemies = enemyDataList.FindAll(e => e.level <= currentLevel);
        if (validEnemies.Count == 0) return; // スポーンできる敵がいない場合は終了

        // ランダムに敵を選択
        EnemyData selectedEnemyData = validEnemies[Random.Range(0, validEnemies.Count)];

        // スポーン位置を計算
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0,
            randomDirection.y * randomDistance
        ) + player.transform.position;

        // プールから取得
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
