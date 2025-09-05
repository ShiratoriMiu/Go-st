using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [SerializeField] Text coinText;

    private void OnEnable()
    {
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        coinText.text = SaveManager.LoadCoin().ToString();
    }
}
