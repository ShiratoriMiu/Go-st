using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadItemData : MonoBehaviour
{
    public static LoadItemData Instance { get; private set; }

    private bool _initialized = false;

    [SerializeField]
    SkinItemTarget skinItemTarget;
    [SerializeField]
    ColorChanger colorChanger;
    [SerializeField]
    MakeUpManager makeUpManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!_initialized)
        {
            _initialized = true;
            InitializeOnce(); // ãNìÆéûÇ…1âÒÇæÇØåƒÇŒÇÍÇÈèàóù
        }
    }

    private void InitializeOnce()
    {
        foreach(var skinItem in skinItemTarget.ItemSlots)
        {
            SaveManager.SaveAllItemName(skinItem.itemName);
        }
        foreach (var colorItem in colorChanger.SkinSlots)
        {
            SaveManager.SaveAllItemName(colorItem.name);
        }
        foreach (var makeUpItem in makeUpManager.MakeUpSlots)
        {
            SaveManager.SaveAllItemName(makeUpItem.name);
        }
    }

}
