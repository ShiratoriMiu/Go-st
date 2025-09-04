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

    private string itemName;

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

    public void ShowShopPop(Sprite sprite, Color bgColor, string text, int coinNum, Action onBuy)
    {
        shopPop.SetActive(true);
        iconImage.sprite = sprite;
        iconBG.color = bgColor;
        iconText.text = text;
        coinText.text = coinNum.ToString();

        onBuyAction = onBuy; // コールバック登録
    }

    public void CloseShopPop()
    {
        shopPop.SetActive(false);
    }

    public void BuyItem()
    {
        Debug.Log("アイテム購入処理");
        onBuyAction?.Invoke(); // 外から渡された処理を実行
    }
}
