using DG.Tweening;
using UnityEngine;
using static UnityEditor.Progress;

public class BuffItem : MonoBehaviour
{
    public BuffSO buffData;

    private void OnEnable()
    {
        SpawnBuff();
    }

    private void SpawnBuff()
    {
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if(buffData.canGetSkill == false && player.GetIsSkill()) return;
            if (player != null && buffData != null)
            {
                player.ApplyBuff(buffData);
                Debug.Log($"バフ「{buffData.buffType}」を適用");

                if(buffData.buffType == BuffType.Heal)
                {
                    SoundManager.Instance.PlaySE("HealSE");
                }
                else if(buffData.buffType == BuffType.SpeedBoost)
                {
                    SoundManager.Instance.PlaySE("SpeedBoostSE");
                }

                Destroy(this.gameObject); // アイテムを消す
            }
        }
    }
}
