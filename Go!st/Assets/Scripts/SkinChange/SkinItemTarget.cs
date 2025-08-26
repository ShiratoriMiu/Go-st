using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SkinItemTarget : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlot
    {
        public string itemName;
        public Renderer[] itemObjectRenderers;
        public Sprite itemIcon;
        public bool canColorChange;
        public bool isOwned;
        public bool isEquipped;
    }

    [SerializeField]
    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<string> activeItemNames = new List<string>();
    
    public IReadOnlyList<ItemSlot> ItemSlots => itemSlots;

    public System.Action<ItemSlot> OnItemEquipped;
    public System.Action<ItemSlot> OnItemUnequipped;

    [SerializeField] int maxItemNum = 2;//�����ł���ő�̃A�C�e����

    private void Start()
    {
        RefreshActiveItems();
    }

    public void ToggleItem(ItemSlot _item, Action<int, int> _updateItemNum, Action buttonPressedColor, Action buttonColorReset)
    {
        if (activeItemNames.Contains(_item.itemName))
        {
            activeItemNames.Remove(_item.itemName);
            for(int i = 0; i < _item.itemObjectRenderers.Length; i++)
            {
                _item.itemObjectRenderers[i].enabled = false;
            }
            
            Debug.Log($"�O����: {_item.itemName}");
            buttonColorReset();
            _updateItemNum(-1, maxItemNum);//itemNumTMP�̍X�V�A1�Ԗڂ̈����͑����̃R�X�g�ɓ����邪����ꗥ��1
            _item.isEquipped = false;
            // ���������ʒm
            OnItemUnequipped?.Invoke(_item);
        }
        else
        {
            if (activeItemNames.Count >= maxItemNum)
            {
                Debug.Log("����ȏ㑕���ł��܂���");
                return;
            }

            activeItemNames.Add(_item.itemName);
            for (int i = 0; i < _item.itemObjectRenderers.Length; i++)
            {
                _item.itemObjectRenderers[i].enabled = true;
            }
            Debug.Log($"��������: {_item.itemName}");
            buttonPressedColor();
            _updateItemNum(1, maxItemNum);
            _item.isEquipped = true;
            // �R�[���o�b�N�Ăяo��
            OnItemEquipped?.Invoke(_item);
        }
    }

    public void HideAllItems()
    {
        foreach (var item in itemSlots)
        {
            for (int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                item.itemObjectRenderers[i].enabled = false;
            }
        }
        activeItemNames.Clear();
    }

    //�A�C�e���̃A�N�e�B�u��Ԃ�؂�ւ�
    private void RefreshActiveItems()
    {
        activeItemNames.Clear();
        foreach (var item in itemSlots)
        {
            bool isItemActive = true;

            for (int i = 0; i < item.itemObjectRenderers.Length; i++)
            {
                if (!item.itemObjectRenderers[i].enabled) isItemActive = false;
            }

            if (item.itemObjectRenderers != null && isItemActive)
            {
                activeItemNames.Add(item.itemName);
            }
        }
    }

    public void EquippedSkinItem(ItemSlot _item)
    {
        activeItemNames.Add(_item.itemName);
        for (int i = 0; i < _item.itemObjectRenderers.Length; i++)
        {
            _item.itemObjectRenderers[i].enabled = true;
        }
        Debug.Log($"��������: {_item.itemName}");
        _item.isEquipped = true;
        // �R�[���o�b�N�Ăяo��
        OnItemEquipped?.Invoke(_item);
    }
}
