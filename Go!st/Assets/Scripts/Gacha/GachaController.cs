using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    public static GachaController Instance { get; private set; }

    public int pullNum { private set; get; }

    public int pullCoinNum = 0;//1回ガチャを引くのに必要なコイン枚数

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 既に存在する場合は破棄
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // シーン切り替えでも破棄されない
    }

    public void PullGacha(int _pullNum)
    {
        pullNum = _pullNum;
        int coinNum = SaveManager.LoadCoin();
        coinNum -= pullCoinNum * pullNum;
        if (coinNum < 0) coinNum = 0;
        SaveManager.SaveCoin(coinNum);
    }
}
