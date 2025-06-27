using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadEffectManager : MonoBehaviour
{
    [SerializeField] GameObject deathEffectPrefab;
    public int poolSize = 10; // �G���ɍ��킹�Đݒ�

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

                // ParticleSystem�p
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    StartCoroutine(DisableAfterTime(obj, ps.main.duration));
                }
                else
                {
                    // Animator�p�̏ꍇ�̓A�j���[�V�����C�x���g�Ŕ�A�N�e�B�u�����Ă�OK
                    StartCoroutine(DisableAfterTime(obj, 1f)); // ����1�b
                }
                return;
            }
        }

        Debug.LogWarning("�v�[��������܂���BpoolSize�𑝂₵�Ă��������B");
    }

    private System.Collections.IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
