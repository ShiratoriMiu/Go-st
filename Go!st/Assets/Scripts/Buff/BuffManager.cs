using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BuffManager : MonoBehaviour
{
    [SerializeField] GameObject[] buffPrefabs;   // ��������I�u�W�F�N�g�̌��
    [SerializeField] Transform spawnAreaRandA;   // �͈�A
    [SerializeField] Transform spawnAreaRandB;   // �͈�B
    [SerializeField] float spawnInterval = 5f;   // �����Ԋu�i�b�j
    [SerializeField] int spawnPerCycle = 2;      // 1��̐�����
    [SerializeField] int maxBuffCount = 10;      // �t�B�[���h��̍ő吔
    [SerializeField] float minDistance = 1.0f;   // �A�C�e�����m�̍Œ዗��
    [SerializeField] float buffLifetime = 10f;   // �������������܂ł̕b��

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
            spawnedBuffs.Add(buff);

            BuffCleaner cleaner = buff.AddComponent<BuffCleaner>();
            cleaner.onDestroy = () => spawnedBuffs.Remove(buff);

            Destroy(buff, buffLifetime);
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

            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            float z = Random.Range(minZ, maxZ);

            Vector3 randomPosition = new Vector3(x, y, z);

            bool overlap = false;
            foreach (var buff in spawnedBuffs)
            {
                if (buff == null) continue;
                if (Vector3.Distance(buff.transform.position, randomPosition) < minDistance)
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
        return Vector3.zero;
    }

    // ���ׂẴo�t���폜
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
