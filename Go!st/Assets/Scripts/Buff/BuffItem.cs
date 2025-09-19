using UnityEngine;

public class BuffItem : MonoBehaviour
{
    public BuffSO buffData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if(buffData.canGetSkill == false && player.GetIsSkill()) return;
            if (player != null && buffData != null)
            {
                player.ApplyBuff(buffData);
                Debug.Log($"�o�t�u{buffData.buffType}�v��K�p");

                if(buffData.buffType == BuffType.Heal)
                {
                    SoundManager.Instance.PlaySE("HealSE");
                }
                else if(buffData.buffType == BuffType.SpeedBoost)
                {
                    SoundManager.Instance.PlaySE("SpeedBoostSE");
                }

                Destroy(gameObject); // �A�C�e��������
            }
        }
    }
}
