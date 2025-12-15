using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ColorChanger;

public class ItemColorChanger : MonoBehaviour
{
    [SerializeField] SkinItemUIManager skinItemUIManager;
    [SerializeField] GameObject itemColorScrollView;

    [System.Serializable]
    public class ItemColorChangeSlot
    {
        public ItemColorChangeSlotColor colorName;
        public Button button;
    }
    [SerializeField] ItemColorChangeSlot[] itemColorChangeSlots;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var itemColorChangeSlot in itemColorChangeSlots)
        {
            if (itemColorChangeSlot != null)
            {
                var capturedItemColorButton = itemColorChangeSlot.button; // ラムダ内でキャプチャするため

                itemColorChangeSlot.button.onClick.AddListener(() =>
                {
                    OnClickItemColorButton(capturedItemColorButton);
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //itemColorScrollViewの表示非表示切替
        if (skinItemUIManager != null && skinItemUIManager.selectItem != null)
        {
            if (!itemColorScrollView.activeSelf)
            {
                itemColorScrollView.SetActive(true);
                RefreshColorSlots();
            }
        }
        else
        {
            if (itemColorScrollView.activeSelf) itemColorScrollView.SetActive(false);
        }
    }

    void OnClickItemColorButton(Button _itemColorButton)
    {
        if (skinItemUIManager.selectItem == null) return;

        for(int i = 0; i < skinItemUIManager.selectItem.itemObjectRenderers.Length; ++i)
        {
            Material matInstance = new Material(skinItemUIManager.selectItem.itemObjectRenderers[i].material); // マテリアルの複製

            Image image = _itemColorButton.GetComponent<Image>();// Imageの使用

            if (image != null)
            {
                matInstance.color = image.color;
            }

            skinItemUIManager.selectItem.itemObjectRenderers[i].material = matInstance;
        }
    }

    void RefreshColorSlots()
    {
        if (skinItemUIManager == null || skinItemUIManager.selectItem == null)
            return;

        string itemName = skinItemUIManager.selectItem.itemName;
        // 解放済み色を取得
        List<string> unlockedColors =
            SaveManager.LoadUnlockedColors(itemName);

        Debug.Log("開放済み色" + unlockedColors + "ここまで");

        foreach (var slot in itemColorChangeSlots)
        {
            bool unlocked = unlockedColors != null &&
                            unlockedColors.Contains(slot.colorName.ToString());

            // ボタンのON/OFF
            slot.button.interactable = unlocked;

            // 見た目も変える
            slot.button.gameObject.SetActive(unlocked);
        }
    }
}
