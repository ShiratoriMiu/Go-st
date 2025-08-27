using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    public static GachaController Instance { get; private set; }

    public int pullNum { private set; get; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͔j��
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // �V�[���؂�ւ��ł��j������Ȃ�
    }

    public void PullGacha(int _pullNum)
    {
        pullNum = _pullNum;
    }
}
