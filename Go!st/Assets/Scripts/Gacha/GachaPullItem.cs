using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GachaPullItem : MonoBehaviour
{
    [SerializeField] GameObject itemIconPrefab;

    [SerializeField] Transform itemIconBase;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(WaitForInitializeAndPull());
    }

    private IEnumerator WaitForInitializeAndPull()
    {
        // LoadItemData が存在して初期化完了するまで待機
        yield return new WaitUntil(() =>
            LoadItemData.Instance != null && LoadItemData.Instance.IsInitialized);

        Debug.Log("アイテムデータ初期化完了 → ガチャ開始");
        PullItem();
    }

    void PullItem()
    {
        // アイテム名リストを取得
        List<ItemData> allItems = SaveManager.AllItems();

        // 結果を入れるリスト
        List<string> pulledItems = new List<string>();

        // pullNum 回ランダムに選ぶ
        for (int i = 0; i < GachaController.Instance.pullNum; i++)
        {
            int randIndex = Random.Range(0, allItems.Count);
            Debug.Log("全アイテムリスト" + allItems.Count);
            string selectedItem = allItems[randIndex].name;
            pulledItems.Add(selectedItem);//獲得したアイテムの名前を登録

            GameObject icon = Instantiate(itemIconPrefab, itemIconBase);//結果表示用のアイコンを生成
            //アイコンの背景の色を変更
            icon.GetComponent<Image>().color = new Color(allItems[randIndex].color[0], allItems[randIndex].color[1], allItems[randIndex].color[2], allItems[randIndex].color[3]);
            //アイコンの子のSpriteを変更
            Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != icon);
            Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(txt => txt.gameObject != icon);
            img.sprite = !string.IsNullOrEmpty(allItems[randIndex].IconName)
                ? Resources.Load<Sprite>($"Icon/{allItems[randIndex].IconName}")
                : null;
            if(img.sprite == null)
            {
                img.color = new Color(0, 0, 0, 0);
                if(allItems[randIndex].IconName != "Null") txt.text = allItems[randIndex].IconName;
            }
            else
            {
                txt.text = "";
            }

            if (allItems[randIndex].canColorChange)
            {
                if (!allItems[randIndex].isColorChangeOn) 
                { 
                    SaveManager.UpdateItemFlags(selectedItem, colorChangeOn: true); 
                }
                else
                {
                    ReturnCoin();
                }
            }
            else
            {
                ReturnCoin();
            }
        }

        // 出力
        Debug.Log("ガチャ結果:");
        foreach (var item in pulledItems)
        {
            Debug.Log(item);
            SaveManager.UpdateItemFlags(item, owned:true);
        }
    }

    private void ReturnCoin()
    {
        int coinNum = SaveManager.LoadCoin();
        coinNum += 200;
        SaveManager.SaveCoin(coinNum);
    }
}
