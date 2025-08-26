using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaController : MonoBehaviour
{
    [SerializeField] Button pull1GachaButton;
    [SerializeField] Button pull9GachaButton;

    [SerializeField] int pullCoinNum = 0;//1回ガチャを引くのに必要なコイン枚数

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
        // アイテム名リストを取得
        List<string> allItems = SaveManager.AllItemNames();

        // 結果を入れるリスト
        List<string> pulledItems = new List<string>();

        // pullNum 回ランダムに選ぶ
        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, allItems.Count);
            string selectedItem = allItems[randIndex];
            pulledItems.Add(selectedItem);

            GameObject icon = Instantiate(itemIconPrefab, itemIconViewBase);
            
        }

        // 出力
        Debug.Log("ガチャ結果:");
        foreach (var item in pulledItems)
        {
            Debug.Log(item);
            SaveManager.SaveOwnedItemName(item);
        }
    }
}
