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

        InitializeOnce(); // 起動時に1回だけ呼ばれる処理

    }

    public void InitializeOnce()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            string itemName = string.IsNullOrEmpty(skinItem.itemName) ? "" : skinItem.itemName;
            string iconName = skinItem.itemIcon == null ? "" : skinItem.itemIcon.name;

            SaveManager.SaveAllItem(itemName, iconName, Color.white, skinItem.isOwned, skinItem.isEquipped, skinItem.canColorChange,skinItem.currentColorChange, ItemStyle.SkinItem,skinItem.colorComplete,skinItem.defaultColor);
            if (skinItem.isGacha)
            {
                SaveManager.SaveGachaItemName(itemName);
            }
        }

        foreach (var colorItem in colorChanger.SkinSlots)
        {
            string itemName = string.IsNullOrEmpty(colorItem.name) ? "" : colorItem.name;
            string iconName = colorItem.icon == null ? "" : colorItem.icon.name;
            //色変え機能はついてないので直接false
            //装備中判定も使用していないのでfalse
            SaveManager.SaveAllItem(itemName, iconName, colorItem.color, colorItem.isOwned,false, false, false, ItemStyle.SkinColor,false, ItemColorChangeSlotColor.White);
            if (colorItem.isGacha)
            {
                SaveManager.SaveGachaItemName(itemName);
            }
        }

        foreach (var makeUpItem in makeUpManager.MakeUpSlots)
        {
            string itemName = string.IsNullOrEmpty(makeUpItem.name) ? "" : makeUpItem.name;
            string iconName = makeUpItem.icon == null ? itemName : makeUpItem.icon.name;
            //色変え機能はついてないので直接false
            SaveManager.SaveAllItem(itemName, iconName, Color.white,makeUpItem.isOwned,makeUpItem.isEquipped,false,false, ItemStyle.MakeUp,false,ItemColorChangeSlotColor.White);
            if (makeUpItem.isGacha)
            {
                SaveManager.SaveGachaItemName(itemName);
            }
        }

        IsInitialized = true; // ? 初期化完了フラグを立てる
    }
}