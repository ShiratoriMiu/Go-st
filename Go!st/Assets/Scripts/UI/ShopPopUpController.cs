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
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͔j��
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

        onBuyAction = _onBuy; // �R�[���o�b�N�o�^
    }

    public void CloseShopPop()
    {
        shopPop.SetActive(false);
    }

    public void BuyItem()
    {
        Debug.Log("�A�C�e���w������");
        int nowCoinNum = SaveManager.LoadCoin();
        nowCoinNum -= coinNum;
        SaveManager.SaveCoin(nowCoinNum);
        onBuyAction?.Invoke(); // �O����n���ꂽ���������s
    }
}
