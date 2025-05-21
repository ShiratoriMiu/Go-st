using System.Collections.Generic;
using UnityEngine;

public class SkinItemTarget : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlot
    {
        public string itemName;
        public GameObject itemObject;
        public Sprite itemIcon;
    }

    [SerializeField]
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<string> activeItemNames = new List<string>();
    
    public IReadOnlyList<ItemSlot> ItemSlots => itemSlots;

    private void Start()
    {
        RefreshActiveItems();
    }

    public void ToggleItem(ItemSlot item)
    {
        if (activeItemNames.Contains(item.itemName))
        {
            activeItemNames.Remove(item.itemName);
            item.itemObject.SetActive(false);
            Debug.Log($"外した: {item.itemName}");
        }
        else
        {
            if (activeItemNames.Count >= 2)
            {
                Debug.Log("これ以上装備できません");
                return;
            }

            activeItemNames.Add(item.itemName);
            item.itemObject.SetActive(true);
            Debug.Log($"装備した: {item.itemName}");
        }
    }

    public void HideAllItems()
    {
        foreach (var item in itemSlots)
        {
            item.itemObject.SetActive(false);
        }
        activeItemNames.Clear();
    }

    //アイテムのアクティブ状態を切り替え
    private void RefreshActiveItems()
    {
        activeItemNames.Clear();
        foreach (var item in itemSlots)
        {
            if (item.itemObject != null && item.itemObject.activeSelf)
            {
                activeItemNames.Add(item.itemName);
            }
        }
    }
}
