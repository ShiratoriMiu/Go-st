using UnityEngine;

[CreateAssetMenu(fileName = "NewBuff", menuName = "Buff/BuffEffect")]
public class BuffSO : ScriptableObject
{
    public string buffName = "New Buff";
    public int healAmount = 0;
    public float speedMultiplier = 1f;
    public float duration = 5f;
    public bool canGetSkill = true;//必殺技中に取得できるアイテムかどうか
}
