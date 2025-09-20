using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BuffManager : MonoBehaviour
{
    [SerializeField] GameObject[] buffPrefabs;
    [SerializeField] Transform spawnAreaRandA;
    [SerializeField] Transform spawnAreaRandB;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] int spawnPerCycle = 2;
    [SerializeField] int maxBuffCount = 10;
    [SerializeField] float minDistance = 1.0f;
    [SerializeField] float buffLifetime = 10f;

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
            Vector3 spawnPos = GetValidPosition();
            if (spawnPos == Vector3.zero) continue;

            int prefabIndex = Random.Range(0, buffPrefabs.Length);
            GameObject buff = Instantiate(buffPrefabs[prefabIndex], spawnPos, Quaternion.identity);

            // ÉäÉXÉgÇ…í«â¡
            spawnedBuffs.Add(buff);

            // BuffCleaner Çê∂ê¨íºå„Ç…í«â¡
            BuffCleaner cleaner = buff.AddComponent<BuffCleaner>();
            cleaner.onDestroy = () => spawnedBuffs.Remove(buff);

            // Destroy Ç Coroutine Ç≈êßå‰ÇµÇƒämé¿Ç… onDestroy Ç™åƒÇŒÇÍÇÈÇÊÇ§Ç…Ç∑ÇÈ
            StartCoroutine(DestroyAfter(buff, buffLifetime));
        }
    }

    IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    Vector3 GetValidPosition()
    {
        const int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            float minX = Mathf.Min(spawnAreaRandA.position.x, spawnAreaRandB.position.x);
            float maxX = Mathf.Max(spawnAreaRandA.position.x, spawnAreaRandB.position.x);
            float minY = Mathf.Min(spawnAreaRandA.position.y, spawnAreaRandB.position.y);
            float maxY = Mathf.Max(spawnAreaRandA.position.y, spawnAreaRandB.position.y);
            float minZ = Mathf.Min(spawnAreaRandA.position.z, spawnAreaRandB.position.z);
            float maxZ = Mathf.Max(spawnAreaRandA.position.z, spawnAreaRandB.position.z);

            Vector3 randomPos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                Random.Range(minZ, maxZ)
            );

            bool overlap = false;
            foreach (var buff in spawnedBuffs)
            {
                if (buff == null) continue;
                if (Vector3.Distance(buff.transform.position, randomPos) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap) return randomPos;
        }
        return Vector3.zero;
    }

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
