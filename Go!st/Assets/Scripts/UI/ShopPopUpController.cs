using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopUpController : MonoBehaviour
{
    public static ShopPopUpController Instance { get; private set; }

    [SerializeField]
    private GameObject shopPop;

    [SerializeField] Image iconImage;
    [SerializeField] Image iconBG;
    [SerializeField] Text iconText;
    [SerializeField] Text coinText;

    [SerializeField] Button buyButton;

    private string itemName;
    private int coinNum = 0;

    private Action onBuyAction;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 既に存在する場合は破棄
            return;
        }

        Instance = this;
        shopPop.SetActive(false);
    }

    public void ShowShopPop(Sprite _sprite, Color _bgColor, string _text, int _coinNum, Action _onBuy)
    {
        shopPop.SetActive(true);
        iconImage.sprite = _sprite;
        iconBG.color = _bgColor;
        iconText.text = _text;
        coinText.text = _coinNum.ToString();
        coinNum = _coinNum;

        if (coinNum > SaveManager.LoadCoin())
        {
            buyButton.interactable = false;
        }
        else
        {
            buyButton.interactable= true;
        }

        onBuyAction = _onBuy; // コールバック登録
    }

    public void CloseShopPop()
    {
        shopPop.SetActive(false);
    }

    public void BuyItem()
    {
        Debug.Log("アイテム購入処理");
        int nowCoinNum = SaveManager.LoadCoin();
        nowCoinNum -= coinNum;
        SaveManager.SaveCoin(nowCoinNum);
        onBuyAction?.Invoke(); // 外から渡された処理を実行
    }
}
