using UnityEngine;

[CreateAssetMenu(fileName = "NewBuff", menuName = "Buff/BuffEffect")]
public class BuffSO : ScriptableObject
{
    public BuffType buffType; // Å©í«â¡

    public int healAmount = 0;
    public float speedMultiplier = 1f;
    public float skillCoolTimeAdd = 1f;
    public float duration = 5f;
    public bool canGetSkill = true;
}

public enum BuffType
{
    Heal,
    SpeedBoost,
    SkillCoolTime,
}
