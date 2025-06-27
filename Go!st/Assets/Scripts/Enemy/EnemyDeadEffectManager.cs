using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadEffectManager : MonoBehaviour
{
    [SerializeField] GameObject deathEffectPrefab;
    public int poolSize = 10; // 敵数に合わせて設定

    private List<GameObject> effectPool = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(deathEffectPrefab);
            obj.SetActive(false);
            effectPool.Add(obj);
        }
    }

    public void PlayEffect(Vector3 position)
    {
        foreach (var obj in effectPool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.SetActive(true);

                // ParticleSystem用
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    StartCoroutine(DisableAfterTime(obj, ps.main.duration));
                }
                else
                {
                    // Animator用の場合はアニメーションイベントで非アクティブ化してもOK
                    StartCoroutine(DisableAfterTime(obj, 1f)); // 仮で1秒
                }
                return;
            }
        }

        Debug.LogWarning("プールが足りません。poolSizeを増やしてください。");
    }

    private System.Collections.IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
