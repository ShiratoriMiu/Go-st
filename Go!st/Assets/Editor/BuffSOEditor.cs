using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuffSO))]
public class BuffSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var buff = (BuffSO)target;

        // �o�t�^�C�v��I��
        buff.buffType = (BuffType)EditorGUILayout.EnumPopup("Buff Type", buff.buffType);

        buff.canGetSkill = EditorGUILayout.Toggle("Can Get Skill", buff.canGetSkill);

        // �o�t�^�C�v�ɂ���ĕ\�����ڂ�؂�ւ�
        switch (buff.buffType)
        {
            case BuffType.Heal:
                buff.healAmount = EditorGUILayout.IntField("Heal Amount", buff.healAmount);
                break;

            case BuffType.SpeedBoost:
                buff.speedMultiplier = EditorGUILayout.FloatField("Speed Multiplier", buff.speedMultiplier);
                buff.duration = EditorGUILayout.FloatField("Duration", buff.duration);
                break;

            case BuffType.SkillCoolTime:
                buff.skillCoolTimeAdd = EditorGUILayout.FloatField("Skill CoolTime Add", buff.skillCoolTimeAdd);
                break;
        }

        // �ύX��ۑ�
        if (GUI.changed)
        {
            EditorUtility.SetDirty(buff);
        }
    }
}
