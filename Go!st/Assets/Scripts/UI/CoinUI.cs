using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [SerializeField] Text coinText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateCoinUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateCoinUI()
    {
        coinText.text = SaveManager.LoadCoin() + "Coin";
    }
}
