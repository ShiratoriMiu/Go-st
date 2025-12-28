using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        shopButton = GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var item = ItemChecker();
        if (item != null)
        {
            SetIcon(item);
        }
        else
        {
            Debug.LogWarning($"Item not found: {selectItemName}");
            // 必要ならデフォルト表示をセット
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconText.text = selectItemName.ToString();
            iconBG.color = Color.white;
        }

        shopButton = GetComponent<Button>();
        shopButton.onClick.AddListener(() =>
        {
            ShopPopUpController.Instance.ShowShopPop(
                iconImage.sprite, iconBG.color, iconText.text, coinNum, Buy);
        });
    }

    private void OnEnable()
    {
        if (shopButton == null) shopButton = GetComponent<Button>();
        ButtonOff();
    }

    void ButtonOff()
    {
        var item = ItemChecker();
        if (item != null && item.isOwned)
            shopButton.interactable = false;
    }

    private void SetIcon(ItemData _item)
    {
        Sprite iconSprite = Resources.Load<Sprite>($"Icon/{_item.IconName}");

        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
            iconText.text = "";
            iconBG.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconText.text = _item.name;
            iconBG.color = _item.ToColor();
        }
        
    }

    private ItemData ItemChecker()
    {
        foreach(var item in SaveManager.AllItems())
        {
            if(item.name == selectItemName.ToString())
            {
                return item;
            }
        }
        return null;
    }

    private void Buy()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            if (skinItem.itemName == selectItemName.ToString())
            {
                skinItem.isOwned = true;
                skinItem.currentColorChange = true;
                foreach (ItemColorChangeSlotColor color in System.Enum.GetValues(typeof(ItemColorChangeSlotColor)))
                {
                    string colorName = color.ToString();
                    SaveManager.SaveUnlockedColor(skinItem.itemName, colorName);
                }
                SaveManager.UpdateItemColorComplete(skinItem.itemName);
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
        ButtonOff();
    }
}
