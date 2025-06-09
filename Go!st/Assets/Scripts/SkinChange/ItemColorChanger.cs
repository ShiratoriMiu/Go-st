using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ColorChanger;

public class ItemColorChanger : MonoBehaviour
{
    [SerializeField] SkinItemUIManager skinItemUIManager;
    [SerializeField] GameObject itemColorScrollView;

    [SerializeField] Button[] itemColorButtons;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var button in itemColorButtons)
        {
            if (button != null)
            {
                var capturedItemColorButton = button; // �����_���ŃL���v�`�����邽��

                button.onClick.AddListener(() =>
                {
                    OnClickItemColorButton(capturedItemColorButton);
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //itemColorScrollView�̕\����\���ؑ�
        if (skinItemUIManager != null && skinItemUIManager.selectItem != null)
        {
            if(!itemColorScrollView.activeSelf) itemColorScrollView.SetActive(true);
        }
        else
        {
            if (itemColorScrollView.activeSelf) itemColorScrollView.SetActive(false);
        }
    }

    void OnClickItemColorButton(Button _itemColorButton)
    {
        if (skinItemUIManager.selectItem == null) return;

        for(int i = 0; i < skinItemUIManager.selectItem.Length; ++i)
        {
            Material matInstance = new Material(skinItemUIManager.selectItem[i].material); // �}�e���A���̕���

            Image image = _itemColorButton.GetComponent<Image>();// Image�̎g�p

            if (image != null)
            {
                matInstance.color = image.color;
            }

            skinItemUIManager.selectItem[i].material = matInstance;
        }
    }
}
