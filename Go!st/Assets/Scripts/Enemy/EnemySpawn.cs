using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField,Header("敵のプレハブ")] GameObject enemyPrefab; // 敵のプレハブ
    [SerializeField, Header("敵の最大数")] private int maxPoolSize = 100; // プールの最大サイズ
    [SerializeField, Header("次にスポーンするまでの時間")] private float spawnTime = 10f;
    //プレイヤーを基準に周りにスポーン
    [SerializeField, Header("最小スポーン位置")] private float spawnMinDistance = 5f;
    [SerializeField, Header("最大スポーン位置")] private float spawnMaxDistance = 10f;

    private GameObject player;
    private float time = 0;
    private PlayerController playerController;

    private List<GameObject> enemyPool; // 敵オブジェクトのプール

    void Start()
    {
        // プレイヤーを取得
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        // プールを初期化
        enemyPool = new List<GameObject>();
        for (int i = 0; i < maxPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab,this.transform);
            enemy.SetActive(false); // 初期状態で非アクティブにする
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
        // ランダムな方向を生成 (XZ平面)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // ランダムな距離を生成
        float randomDistance = Random.Range(spawnMinDistance, spawnMaxDistance);

        // オフセットを計算
        Vector3 spawnPosition = new Vector3(
            randomDirection.x * randomDistance,
            0, // 高さは必要に応じて調整
            randomDirection.y * randomDistance
        );

        // プレイヤーの位置を基準に配置
        spawnPosition += player.transform.position;

        // プールから非アクティブな敵を取得
        GameObject enemy = GetPooledEnemy();
        if (enemy != null)
        {
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = Quaternion.identity;
            enemy.SetActive(true); // 敵をアクティブにする
        }
    }

    // プールから非アクティブな敵を取得
    GameObject GetPooledEnemy()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy) // 非アクティブなオブジェクトを探す
            {
                return enemy;
            }
        }
        return null; // プールがすべて使用中の場合は null を返す
    }
}
