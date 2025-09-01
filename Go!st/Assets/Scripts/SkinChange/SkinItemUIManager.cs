using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinItemUIManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemButton
    {
        public Button button;
        public Image icon;
        [HideInInspector] public TogglePressedLookButton togglePressedLook;
    }

    [SerializeField] GameObject scrollViewIconPrefab;
    [SerializeField] GameObject skinItemScrollViewBase;

    private List<ItemButton> itemButtons;

    private SkinItemTarget currentTarget;

    public Renderer[] selectItem { get; private set; }

    [SerializeField] private ItemNumTMP itemNumTMP;

    private void Start()
    {
        selectItem = null;

        foreach(ItemButton itemButton in itemButtons)
        {
            itemButton.togglePressedLook = itemButton.button.GetComponent<TogglePressedLookButton>();
        }
    }

    public void SetTargetPlayer(SkinItemTarget target)
    {
        currentTarget = target;

        GenerateOwnedItemButtons();

        currentTarget.OnItemEquipped = (item) => {
            var renderer = item.itemObjectRenderers;
            if (renderer != null)
            {
                if(item.canColorChange && item.currentColorChange) selectItem = renderer;
                else selectItem = null;
            }
        };

        currentTarget.OnItemUnequipped = (item) => {
            selectItem = null;
        };
    }

    public void GenerateOwnedItemButtons()
    {
        // 既存のボタンをクリア
        if (itemButtons != null)
        {
            foreach (var btn in itemButtons)
            {
                if (btn.button != null)
                    Destroy(btn.button.gameObject);
            }
            itemButtons.Clear(); // ← これを追加
        }
        else
        {
            itemButtons = new List<ItemButton>();
        }

        foreach (var item in currentTarget.ItemSlots)
        {
            if (!item.isOwned) continue; // 所持していないアイテムはスキップ

            // scrollViewIconPrefabを生成
            GameObject go = Instantiate(scrollViewIconPrefab, skinItemScrollViewBase.transform);
            Button btn = go.GetComponent<Button>();
            // 子オブジェクトのImageのみ取得（ルートのImageは無視）
            Image icon = go.GetComponentsInChildren<Image>()
                            .FirstOrDefault(img => img.gameObject != go);


            ItemButton newItemButton = new ItemButton
            {
                button = btn,
                icon = icon,
                togglePressedLook = btn.GetComponent<TogglePressedLookButton>()
            };

            // ボタンのクリック処理
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                currentTarget.ToggleItem(item,
                    itemNumTMP.UpdateItemNumTMP,
                    newItemButton.togglePressedLook.SetPressedLook,
                    newItemButton.togglePressedLook.ResetButtonLook);
            });

            // アイコン設定
            icon.sprite = item.itemIcon;

            itemButtons.Add(newItemButton);
        }
    }

}
