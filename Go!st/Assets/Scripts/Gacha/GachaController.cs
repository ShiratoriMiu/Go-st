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
            Destroy(gameObject); // Šù‚É‘¶İ‚·‚éê‡‚Í”jŠü
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ƒV[ƒ“Ø‚è‘Ö‚¦‚Å‚à”jŠü‚³‚ê‚È‚¢
    }

    public void PullGacha(int _pullNum)
    {
        pullNum = _pullNum;
    }
}
