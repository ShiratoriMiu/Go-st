using UnityEngine;

public class LoadItemData : MonoBehaviour
{
    public static LoadItemData Instance { get; private set; }

    public bool IsInitialized { get; private set; } = false;

    [SerializeField] SkinItemTarget skinItemTarget;
    [SerializeField] ColorChanger colorChanger;
    [SerializeField] MakeUpManager makeUpManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeOnce(); // ‹N“®‚É1‰ñ‚¾‚¯ŒÄ‚Î‚ê‚éˆ—
    }

    private void InitializeOnce()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            string itemName = string.IsNullOrEmpty(skinItem.itemName) ? "" : skinItem.itemName;
            string iconName = skinItem.itemIcon == null ? "" : skinItem.itemIcon.name;

            SaveManager.SaveAllItem(itemName, iconName, Color.white);
        }

        foreach (var colorItem in colorChanger.SkinSlots)
        {
            string itemName = string.IsNullOrEmpty(colorItem.name) ? "" : colorItem.name;
            string iconName = colorItem.icon == null ? "" : colorItem.icon.name;

            SaveManager.SaveAllItem(itemName, iconName, colorItem.color);
        }

        foreach (var makeUpItem in makeUpManager.MakeUpSlots)
        {
            string itemName = string.IsNullOrEmpty(makeUpItem.name) ? "" : makeUpItem.name;

            SaveManager.SaveAllItem(itemName, itemName, Color.white);
        }

        IsInitialized = true; // ? ‰Šú‰»Š®—¹ƒtƒ‰ƒO‚ğ—§‚Ä‚é
    }
}