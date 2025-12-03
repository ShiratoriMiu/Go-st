using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopIconPlayerIcon : MonoBehaviour
{
    Button shopButton;

    [SerializeField] PlayerIconData playerIcon;

    [SerializeField] Image iconImage;
    [SerializeField] Image iconBG;
    [SerializeField] Text iconText;

    [SerializeField] int coinNum = 0;

    void Awake()
    {
        shopButton = GetComponent<Button>();
        //nameÇ…spriteÇÃñºëOÇê›íË
        if (playerIcon.style == PlayerIconStyle.Chara) playerIcon.name = iconImage.sprite.name;
        else playerIcon.name = iconBG.sprite.name;
    }

    // Start is called before the first frame update
    void Start()
    {
        shopButton = GetComponent<Button>();
        shopButton.onClick.AddListener(() =>
        {
            ShopPopUpController.Instance.ShowShopPop(
                iconImage.sprite, iconBG, iconText.text, coinNum, Buy);
        });
    }

    private void OnEnable()
    {
        if (shopButton == null) shopButton = GetComponent<Button>();
        ButtonOff();
    }

    void ButtonOff()
    {
        List<PlayerIconData> ownedPlayerIcons = SaveManager.LoadOwnedPlayerIcons();
        if (ownedPlayerIcons == null) return;
        foreach (var ownedPlayerIcon in SaveManager.LoadOwnedPlayerIcons())
        {
            if(ownedPlayerIcon.name == playerIcon.name) shopButton.interactable = false;
        }
    }

    private void Buy()
    {
        SaveManager.SaveOwnedPlayerIcon(playerIcon);
        ButtonOff();
    }
}
