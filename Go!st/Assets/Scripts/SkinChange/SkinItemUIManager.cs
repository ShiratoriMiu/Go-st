using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinItemUIManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemButton
    {
        public Button button;
        public Image icon;
    }

    [SerializeField] private ItemButton[] itemButtons;

    private SkinItemTarget currentTarget;

    public Renderer[] selectItem { get; private set; }

    private void Start()
    {
        selectItem = null;
    }

    public void SetTargetPlayer(SkinItemTarget target)
    {
        currentTarget = target;

        currentTarget.OnItemEquipped = (item) => {
            var renderer = item.itemObjectRenderers;
            if (renderer != null)
            {
                selectItem = renderer;
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
                    currentTarget.ToggleItem(item);
                });

                itemButtons[index].button.gameObject.SetActive(true);
            }
            else
            {
                itemButtons[index].button.gameObject.SetActive(false);
            }
        }
    }
}
