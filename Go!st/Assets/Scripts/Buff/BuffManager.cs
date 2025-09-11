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
