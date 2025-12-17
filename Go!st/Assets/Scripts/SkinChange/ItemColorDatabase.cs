using System.Collections.Generic;
using UnityEngine;

public class ItemColorDatabase : MonoBehaviour
{
    public static ItemColorDatabase Instance { get; private set; }

    [SerializeField]
    private List<ItemColorData> itemColorDataList;

    private Dictionary<ItemNameSlot, ItemColorData> dict;

    private void Awake()
    {
        // ---- Singleton ----
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // -------------------

        dict = new Dictionary<ItemNameSlot, ItemColorData>();

        foreach (var data in itemColorDataList)
        {
            data.Initialize();

            if (!dict.ContainsKey(data.itemName))
            {
                dict.Add(data.itemName, data);
            }
            else
            {
                Debug.LogWarning($"ItemColorDatabase èdï°íËã`: {data.itemName}");
            }
        }
    }

    public Sprite GetSprite(ItemNameSlot itemName, ItemColorChangeSlotColor color)
    {
        if (dict.TryGetValue(itemName, out var data))
        {
            return data.GetSprite(color);
        }

        Debug.LogWarning($"ItemColorData Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ: {itemName}");
        return null;
    }
}