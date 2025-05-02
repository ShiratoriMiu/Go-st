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
                Debug.Log($"バフ「{buffData.buffName}」を適用");

                Destroy(gameObject); // アイテムを消す
            }
        }
    }
}
