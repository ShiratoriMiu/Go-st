using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SkinItemTarget : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlot
    {
        public string itemName;
        public Renderer[] itemObjectRenderers;
        public Sprite itemIcon;
        public bool canColorChange;
        public bool isOwned;
        public bool isEquipped;
    }

    [SerializeField]
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<string> activeItemNames = new List<string>();
    
    public IReadOnlyList<ItemSlot> ItemSlots => itemSlots;

    public System.Action<ItemSlot> OnItemEquipped;
    public System.Action<ItemSlot> OnItemUnequipped;

    [SerializeField] int maxItemNum = 2;//装備できる最大のアイテム数

    private void Start()
    {
        RefreshActiveItems();
    }

    public void ToggleItem(ItemSlot _item, Action<int, int> _updateItemNum, Action buttonPressedColor, Action buttonColorReset)
    {
        if (activeItemNames.Contains(_item.itemName))
        {
            activeItemNames.Remove(_item.itemName);
            for(int i = 0; i < _item.itemObjectRenderers.Length; i++)
            {
                _item.itemObjectRenderers[i].enabled = false;
            }
            
            Debug.Log($"外した: {_item.itemName}");
            buttonColorReset();
            _updateItemNum(-1, maxItemNum);//itemNumTMPの更新、1番目の引数は装備のコストに当たるが現状一律で1
            _item.isEquipped = false;
            // 装備解除通知
            OnItemUnequipped?.Invoke(_item);
        }
        else
        {
            if (activeItemNames.Count >= maxItemNum)
            {
                Debug.Log("これ以上装備できません");
                return;
            }

            activeItemNames.Add(_item.itemName);
            for (int i = 0; i < _item.itemObjectRenderers.Length; i++)
            {
                _item.itemObjectRenderers[i].enabled = true;
            }
            Debug.Log($"装備した: {_item.itemName}");
            buttonPressedColor();
            _updateItemNum(1, maxItemNum);
            _item.isEquipped = true;
            // コールバック呼び出し
            OnItemEquipped?.Invoke(_item);
        }
    }

    public void HideAllItems()
    {
        foreach (var item in itemSlots)
        {
            for (int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                item.itemObjectRenderers[i].enabled = false;
            }
        }
        activeItemNames.Clear();
    }

    //アイテムのアクティブ状態を切り替え
    private void RefreshActiveItems()
    {
        activeItemNames.Clear();
        foreach (var item in itemSlots)
        {
            bool isItemActive = true;

            for (int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                if (!item.itemObjectRenderers[i].enabled) isItemActive = false;
            }

            if (item.itemObjectRenderers != null && isItemActive)
            {
                activeItemNames.Add(item.itemName);
            }
        }
    }

    public void EquippedSkinItem(ItemSlot _item)
    {
        activeItemNames.Add(_item.itemName);
        for (int i = 0; i < _item.itemObjectRenderers.Length; i++)
        {
            _item.itemObjectRenderers[i].enabled = true;
        }
        Debug.Log($"装備した: {_item.itemName}");
        _item.isEquipped = true;
        // コールバック呼び出し
        OnItemEquipped?.Invoke(_item);
    }
}
