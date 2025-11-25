using DG.Tweening;
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
        // 状態変化を監視して、Game 以外になったらすぐ削除
        gameManager.OnGameStateChanged += HandleGameStateChanged;
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

            // リストに追加
            spawnedBuffs.Add(buff);

            // BuffCleaner を生成直後に追加
            BuffCleaner cleaner = buff.AddComponent<BuffCleaner>();
            cleaner.onDestroy = () => spawnedBuffs.Remove(buff);

            // Destroy を Coroutine で制御して確実に onDestroy が呼ばれるようにする
            StartCoroutine(DestroyAfter(buff, buffLifetime));
        }
    }

    IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        float elapsed = 0f;

        while (elapsed < delay)
        {
            // ゲームが進行中のときだけ時間を進める
            if (gameManager.state == GameState.Game)
            {
                elapsed += Time.deltaTime;
            }

            yield return null; // 1フレーム待つ
        }

        if (obj != null)
        {
            //Destroy(obj);
            DespawnBuff(obj);
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

    void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Title)
        {
            ClearAllBuffs(); // 状態が変わった瞬間に呼ばれる
        }
    }

    public void DespawnBuff(GameObject item)
    {
        var renderers = item.GetComponentsInChildren<Renderer>();
        var tr = item.transform;

        // マテリアルを保持（複数対応）
        List<Material> mats = new List<Material>();
        foreach (var r in renderers)
        {
            mats.AddRange(r.materials); // materialはインスタンス化される
        }

        Sequence seq = DOTween.Sequence();

        // --- 点滅 ---
        foreach (var m in mats)
        {
            seq.Join(m.DOFade(0.3f, 0.3f).SetLoops(6, LoopType.Yoyo));
        }

        // --- 消滅アニメーション ---
        seq.Append(tr.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack));

        // --- 完了時に破棄 ---
        seq.OnComplete(() =>
        {
            Destroy(item);
        });
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
