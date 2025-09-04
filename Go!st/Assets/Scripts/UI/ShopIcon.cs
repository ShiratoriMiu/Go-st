using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

public class ShopIcon : MonoBehaviour
{
    Button shopButton;

    [SerializeField] ItemNameSlot selectItemName;

    [SerializeField] Image iconImage;
    [SerializeField] Image iconBG;
    [SerializeField] Text iconText;

    [SerializeField] SkinItemTarget skinItemTarget;
    [SerializeField] ColorChanger colorChanger;
    [SerializeField] MakeUpManager makeUpManager;

    [SerializeField] int coinNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        ItemChecker();

        shopButton = this.GetComponent<Button>();

        shopButton.onClick.AddListener(() =>
        {
            ShopPopUpController.Instance.ShowShopPop(iconImage.sprite, iconBG.color, iconText.text, coinNum, Buy);
        });
    }

    private void ItemChecker()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            if (skinItem.itemName == selectItemName.ToString())
            {
                if (skinItem.itemIcon != null)
                {
                    iconImage.sprite = skinItem.itemIcon;
                    iconText.text = "";
                }
                else
                {
                    iconText.text = skinItem.itemName;
                }
                iconBG.color = Color.white;
            }
        }

        foreach (var skinColor in colorChanger.SkinSlots)
        {
            if (skinColor.name == selectItemName.ToString())
            {
                if (skinColor.icon != null)
                {
                    iconImage.sprite = skinColor.icon;
                    iconBG.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                    iconBG.color = skinColor.color;
                }
                iconText.text = "";
            }
        }

        foreach (var make in makeUpManager.MakeUpSlots)
        {
            if (make.name == selectItemName.ToString())
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
                iconBG.color = Color.white;
                iconText.text = make.name;
            }
        }
    }

    private void Buy()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            if (skinItem.itemName == selectItemName.ToString())
            {
                skinItem.isOwned = true;
                skinItem.currentColorChange = true;
            }
        }

        foreach (var skinColor in colorChanger.SkinSlots)
        {
            if (skinColor.name == selectItemName.ToString())
            {
                skinColor.isOwned = true;
            }
        }

        foreach (var make in makeUpManager.MakeUpSlots)
        {
            if (make.name == selectItemName.ToString())
            {
                make.isOwned = true;
            }
        }

        SaveManager.UpdateItemFlags(selectItemName.ToString(), owned: true,colorChangeOn:true);
    }
}
