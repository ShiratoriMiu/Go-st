using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] private float spawnTime = 10f;
    [SerializeField] private float spawnMinDistance = 5f;
    [SerializeField] private float spawnMaxDistance = 10f;

    Transform player;
    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if(time > spawnTime)
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
        spawnPosition += player.position;

        // オブジェクトを生成
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
