using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadEffectManager : MonoBehaviour
{
    [Header("ê›íË")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private float offsetY = 0.5f;

    private Queue<ParticleSystem> effectPool = new Queue<ParticleSystem>();

    void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(effectPrefab, transform);
            obj.SetActive(false);

            ParticleSystem ps = obj.GetComponent<ParticleSystem>();

            var returner = obj.AddComponent<ParticleReturner>();
            returner.Manager = this;
            returner.ParticleSystem = ps;

            effectPool.Enqueue(ps);
        }
    }

    public void PlayEffect(Vector3 position)
    {
        if (effectPool.Count > 0)
        {
            ParticleSystem ps = effectPool.Dequeue();
            ps.transform.position = position + new Vector3(0, offsetY,0);
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        else
        {
            Debug.LogWarning("[EnemyDeadEffectManager] ÉvÅ[ÉãÇ™ïsë´ÇµÇƒÇ¢Ç‹Ç∑");
        }
    }

    public void ReturnToPool(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        effectPool.Enqueue(ps);
    }
}

public class ParticleReturner : MonoBehaviour
{
    public EnemyDeadEffectManager Manager;
    public ParticleSystem ParticleSystem;

    void Update()
    {
        if (ParticleSystem.isPlaying == false && gameObject.activeSelf)
        {
            Manager.ReturnToPool(ParticleSystem);
        }
    }
}
