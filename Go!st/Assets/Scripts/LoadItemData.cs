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

        InitializeOnce(); // �N������1�񂾂��Ă΂�鏈��
    }

    private void InitializeOnce()
    {
        foreach (var skinItem in skinItemTarget.ItemSlots)
        {
            string itemName = string.IsNullOrEmpty(skinItem.itemName) ? "" : skinItem.itemName;
            string iconName = skinItem.itemIcon == null ? "" : skinItem.itemIcon.name;

            SaveManager.SaveAllItem(itemName, iconName, Color.white, skinItem.isOwned, skinItem.isEquipped, skinItem.canColorChange,skinItem.currentColorChange);
        }

        foreach (var colorItem in colorChanger.SkinSlots)
        {
            string itemName = string.IsNullOrEmpty(colorItem.name) ? "" : colorItem.name;
            string iconName = colorItem.icon == null ? "" : colorItem.icon.name;
            //�F�ς��@�\�͂��ĂȂ��̂Œ���false
            //������������g�p���Ă��Ȃ��̂�false
            SaveManager.SaveAllItem(itemName, iconName, colorItem.color, colorItem.isOwned,false, false, false);
        }

        foreach (var makeUpItem in makeUpManager.MakeUpSlots)
        {
            string itemName = string.IsNullOrEmpty(makeUpItem.name) ? "" : makeUpItem.name;
            //�F�ς��@�\�͂��ĂȂ��̂Œ���false
            SaveManager.SaveAllItem(itemName, itemName, Color.white,makeUpItem.isOwned,makeUpItem.isEquipped,false,false);
        }

        IsInitialized = true; // ? �����������t���O�𗧂Ă�
    }
}