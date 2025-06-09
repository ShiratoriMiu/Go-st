using System.Collections.Generic;
using UnityEngine;

public class SkinItemTarget : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlot
    {
        public string itemName;
        public Renderer[] itemObjectRenderers;
        public Sprite itemIcon;
    }

    [SerializeField]
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<string> activeItemNames = new List<string>();
    
    public IReadOnlyList<ItemSlot> ItemSlots => itemSlots;

    public System.Action<ItemSlot> OnItemEquipped;
    public System.Action<ItemSlot> OnItemUnequipped;


    private void Start()
    {
        RefreshActiveItems();
    }

    public void ToggleItem(ItemSlot item)
    {
        if (activeItemNames.Contains(item.itemName))
        {
            activeItemNames.Remove(item.itemName);
            for(int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                item.itemObjectRenderers[i].enabled = false;
            }
            
            Debug.Log($"外した: {item.itemName}");

            // 装備解除通知
            OnItemUnequipped?.Invoke(item);
        }
        else
        {
            if (activeItemNames.Count >= 2)
            {
                Debug.Log("これ以上装備できません");
                return;
            }

            activeItemNames.Add(item.itemName);
            for (int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                item.itemObjectRenderers[i].enabled = true;
            }
            Debug.Log($"装備した: {item.itemName}");

            // コールバック呼び出し
            OnItemEquipped?.Invoke(item);
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
}
