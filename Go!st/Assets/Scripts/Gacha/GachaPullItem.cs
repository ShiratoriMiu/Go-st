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

        //ガチャで出るアイテム名を取得
        List<string> gachaItems = SaveManager.GachaItems();

        //全てのアイテムからガチャで出るアイテムのみに絞る
        List<ItemData> gachaItemDatas = new List<ItemData>();
        if (allItems != null && gachaItems != null)
        {
            foreach (ItemData item in allItems)
            {
                Debug.Log($"[{item.name}] color = {string.Join(",", item.color)}");

                foreach (string gachaItemName in gachaItems) 
                {
                    if (item.name == gachaItemName)gachaItemDatas.Add(item);
                }
            }
        }

        // 結果を入れるリスト
        List<string> pulledItems = new List<string>();

        // pullNum 回ランダムに選ぶ
        for (int i = 0; i < GachaController.Instance.pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            Debug.Log("全アイテムリスト" + allItems.Count);
            Debug.Log("ガチャアイテムリスト" + gachaItemDatas.Count);

            string selectedItem = gachaItemDatas[randIndex].name;
            pulledItems.Add(selectedItem);//獲得したアイテムの名前を登録

            GameObject icon = Instantiate(itemIconPrefab, itemIconBase);//結果表示用のアイコンを生成
            //アイコンの背景の色を変更
            icon.GetComponent<Image>().color = gachaItemDatas[randIndex].ToColor();
            //アイコンの子のSpriteを変更
            Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != icon);
            Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(txt => txt.gameObject != icon);
            img.sprite = !string.IsNullOrEmpty(gachaItemDatas[randIndex].IconName)
                ? Resources.Load<Sprite>($"Icon/{gachaItemDatas[randIndex].IconName}")
                : null;
            if(img.sprite == null)
            {
                img.color = new Color(1, 1, 1, 1);
                if(gachaItemDatas[randIndex].IconName != "Null") txt.text = gachaItemDatas[randIndex].IconName;
            }
            else
            {
                txt.text = "";
            }

            if (gachaItemDatas[randIndex].canColorChange)
            {
                if (!gachaItemDatas[randIndex].isColorChangeOn) 
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
