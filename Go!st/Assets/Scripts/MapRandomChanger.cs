using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRandomChanger : MonoBehaviour
{
    [SerializeField] GameObject[] maps;

    /// <summary>
    /// �����_����1���A�N�e�B�u�ɂ��A����ȊO���\���ɂ���
    /// </summary>
    public void ActivateRandomMap()
    {
        if (maps == null || maps.Length == 0)
        {
            Debug.LogWarning("maps ����ł��B�I�u�W�F�N�g��o�^���Ă��������B");
            return;
        }

        // �܂��S����\���ɂ���
        foreach (GameObject map in maps)
        {
            if (map != null)
                map.SetActive(false);
        }

        // �����_����1�I��ŕ\��
        int randomIndex = Random.Range(0, maps.Length);
        if (maps[randomIndex] != null)
            maps[randomIndex].SetActive(true);
    }
}