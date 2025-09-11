using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BuffManager : MonoBehaviour
{
    [SerializeField] GameObject[] buffPrefabs;   // 生成するオブジェクトの候補
    [SerializeField] Transform spawnAreaRandA;   // 範囲A
    [SerializeField] Transform spawnAreaRandB;   // 範囲B
    [SerializeField] float spawnInterval = 5f;   // 生成間隔（秒）
    [SerializeField] int spawnPerCycle = 2;      // 1回の生成数
    [SerializeField] int maxBuffCount = 10;      // フィールド上の最大数
    [SerializeField] float minDistance = 1.0f;   // アイテム同士の最低距離
    [SerializeField] float buffLifetime = 10f;   // 生成から消えるまでの秒数

    private List<GameObject> spawnedBuffs = new List<GameObject>();

    [SerializeField] GameManager gameManager;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (gameManager.state == GameState.Game)
            {
                SpawnBuffs();
            }
            else
            {
                ClearAllBuffs();
            }
        }
    }

    void SpawnBuffs()
    {
        if (spawnedBuffs.Count >= maxBuffCount) return;

        int spawnCount = Mathf.Min(spawnPerCycle, maxBuffCount - spawnedBuffs.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 spawnPos = GetValidPosition();

            if (spawnPos == Vector2.zero) continue;

            int prefabIndex = Random.Range(0, buffPrefabs.Length);

            GameObject buff = Instantiate(buffPrefabs[prefabIndex], spawnPos, Quaternion.identity);
            spawnedBuffs.Add(buff);

            BuffCleaner cleaner = buff.AddComponent<BuffCleaner>();
            cleaner.onDestroy = () => spawnedBuffs.Remove(buff);

            Destroy(buff, buffLifetime);
        }
    }

    Vector2 GetValidPosition()
    {
        const int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(spawnAreaRandA.position.x, spawnAreaRandB.position.x);
            float y = Random.Range(spawnAreaRandA.position.y, spawnAreaRandB.position.y);
            Vector2 randomPosition = new Vector2(x, y);

            bool overlap = false;
            foreach (var buff in spawnedBuffs)
            {
                if (buff == null) continue;
                if (Vector2.Distance(buff.transform.position, randomPosition) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                return randomPosition;
            }
        }
        return Vector2.zero;
    }

    // すべてのバフを削除
    void ClearAllBuffs()
    {
        foreach (var buff in spawnedBuffs)
        {
            if (buff != null)
            {
                Destroy(buff);
            }
        }
        spawnedBuffs.Clear();
    }
}

public class BuffCleaner : MonoBehaviour
{
    public System.Action onDestroy;
    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
