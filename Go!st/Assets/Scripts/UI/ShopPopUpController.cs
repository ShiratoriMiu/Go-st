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
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͔j��
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

        onBuyAction = onBuy; // �R�[���o�b�N�o�^
    }

    public void CloseShopPop()
    {
        shopPop.SetActive(false);
    }

    public void BuyItem()
    {
        Debug.Log("�A�C�e���w������");
        onBuyAction?.Invoke(); // �O����n���ꂽ���������s
    }
}
