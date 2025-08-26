using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaController : MonoBehaviour
{
    [SerializeField] Button pull1GachaButton;
    [SerializeField] Button pull9GachaButton;

    [SerializeField] int pullCoinNum = 0;//1��K�`���������̂ɕK�v�ȃR�C������

    [SerializeField] GameObject itemIconPrefab;
    [SerializeField] Transform itemIconViewBase;

    // Start is called before the first frame update
    void Start()
    {
        if (SaveManager.LoadCoin() < pullCoinNum)
        {
            pull1GachaButton.interactable = false;
            pull9GachaButton.interactable = false;
        }
        else if(SaveManager.LoadCoin() < pullCoinNum * 9)
        {
            pull9GachaButton.interactable = false;
        }
        else
        {
            pull1GachaButton.interactable = true;
            pull9GachaButton.interactable = true;
        }
    }

    private void PullGacha(int pullNum)
    {
        // �A�C�e�������X�g���擾
        List<string> allItems = SaveManager.AllItemNames();

        // ���ʂ����郊�X�g
        List<string> pulledItems = new List<string>();

        // pullNum �񃉃��_���ɑI��
        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, allItems.Count);
            string selectedItem = allItems[randIndex];
            pulledItems.Add(selectedItem);

            GameObject icon = Instantiate(itemIconPrefab, itemIconViewBase);
            
        }

        // �o��
        Debug.Log("�K�`������:");
        foreach (var item in pulledItems)
        {
            Debug.Log(item);
            SaveManager.SaveOwnedItemName(item);
        }
    }
}
