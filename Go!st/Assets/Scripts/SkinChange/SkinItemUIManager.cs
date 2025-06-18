using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private ItemButton[] itemButtons;

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

        currentTarget.OnItemEquipped = (item) => {
            var renderer = item.itemObjectRenderers;
            if (renderer != null)
            {
                if(item.canColorChange) selectItem = renderer;
                else selectItem = null;
            }
        };

        currentTarget.OnItemUnequipped = (item) => {
            selectItem = null;
        };

        // アイコンの更新とボタンのセット
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;

            if (i < target.ItemSlots.Count)
            {
                var item = target.ItemSlots[index];

                itemButtons[index].icon.sprite = item.itemIcon;
                itemButtons[index].button.onClick.RemoveAllListeners();
                itemButtons[index].button.onClick.AddListener(() => {
                    currentTarget.ToggleItem(item , itemNumTMP.UpdateItemNumTMP, itemButtons[index].togglePressedLook.SetPressedLook, itemButtons[index].togglePressedLook.ResetButtonLook);
                });

                //target.ItemSlots.CountとitemButtonsの数に合わせてActiveを切り替え
                itemButtons[index].button.gameObject.SetActive(true);
            }
            else
            {
                //target.ItemSlots.CountとitemButtonsの数に合わせてActiveを切り替え
                itemButtons[index].button.gameObject.SetActive(false);
            }
        }
    }
}
