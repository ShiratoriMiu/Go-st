using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopPopUpController : MonoBehaviour
{
    public static ShopPopUpController Instance { get; private set; }

    [SerializeField]
    private GameObject shopPopUpBase;
    [SerializeField] RectTransform shopPopUp;

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
            Destroy(gameObject); // Šù‚É‘¶Ý‚·‚éê‡‚Í”jŠü
            return;
        }

        Instance = this;
        ShopPopUpClose();
    }

    public void ShowShopPop(Sprite _sprite, Color _bgColor, string _text, int _coinNum, Action _onBuy)
    {
        ShopPopUpOpen();
        iconImage.sprite = _sprite;
        iconBG.color = _bgColor;
        iconBG.sprite = null;
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

        onBuyAction = _onBuy; // ƒR[ƒ‹ƒoƒbƒN“o˜^
    }

    public void ShowShopPop(Sprite _sprite, Sprite _bgSprite, string _text, int _coinNum, Action _onBuy)
    {
        ShopPopUpOpen();
        iconImage.sprite = _sprite;
        iconBG.sprite = _bgSprite;
        iconBG.color = Color.white;
        iconText.text = _text;
        coinText.text = _coinNum.ToString();
        coinNum = _coinNum;

        if (coinNum > SaveManager.LoadCoin())
        {
            buyButton.interactable = false;
        }
        else
        {
            buyButton.interactable = true;
        }

        onBuyAction = _onBuy; // ƒR[ƒ‹ƒoƒbƒN“o˜^
    }

    public void BuyItem()
    {
        Debug.Log("ƒAƒCƒeƒ€w“üˆ—");
        int nowCoinNum = SaveManager.LoadCoin();
        nowCoinNum -= coinNum;
        SaveManager.SaveCoin(nowCoinNum);
        onBuyAction?.Invoke(); // ŠO‚©‚ç“n‚³‚ê‚½ˆ—‚ðŽÀs
    }

    private void ShopPopUpOpen()
    {
        shopPopUpBase.SetActive(true);

        // ‰ŠúƒXƒP[ƒ‹‚ð0‚É
        shopPopUp.localScale = Vector3.zero;

        // 0 ¨ 1 ‚ÉŠg‘åi0.25•bj
        shopPopUp.DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack); // ­‚µ’e‚ÞŠ´‚¶‚É
    }

    public void ShopPopUpClose()
    {
        // 1 ¨ 0 ‚Ék¬i0.2•bj
        shopPopUp.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                shopPopUpBase.SetActive(false);
            });
    }
}
