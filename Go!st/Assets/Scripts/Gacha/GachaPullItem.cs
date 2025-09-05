using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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

        // ガチャで出るアイテム名を取得
        List<string> gachaItems = SaveManager.GachaItems();

        // 全てのアイテムからガチャで出るアイテムのみに絞る
        List<ItemData> gachaItemDatas = new List<ItemData>();
        if (allItems != null && gachaItems != null)
        {
            foreach (ItemData item in allItems)
            {
                foreach (string gachaItemName in gachaItems)
                {
                    if (item.name == gachaItemName) gachaItemDatas.Add(item);
                }
            }
        }

        // pullNum 回ランダムに選ぶ
        for (int i = 0; i < GachaController.Instance.pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            string selectedItemName = gachaItemDatas[randIndex].name;

            // 毎回最新の状態を取得
            ItemData currentItem = SaveManager.AllItems().First(x => x.name == selectedItemName);

            // アイコン生成
            GameObject icon = Instantiate(itemIconPrefab, itemIconBase);
            Image iconBG = icon.GetComponent<Image>();
            iconBG.color = currentItem.ToColor();

            Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
            Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);
            img.sprite = !string.IsNullOrEmpty(currentItem.IconName)
                ? Resources.Load<Sprite>($"Icon/{currentItem.IconName}")
                : null;

            if (img.sprite == null)
            {
                img.color = new Color(1, 1, 1, 1);
                if (currentItem.IconName != "Null") txt.text = currentItem.IconName;
            }
            else
            {
                txt.text = "";
            }

            // 所持済みアイテムの処理
            if (currentItem.isOwned)
            {
                if (currentItem.canColorChange)
                {
                    // 最新状態で色解放判定
                    if (!currentItem.isColorChangeOn)
                    {
                        SaveManager.UpdateItemFlags(selectedItemName, colorChangeOn: true);
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

            // 所持フラグを更新
            SaveManager.UpdateItemFlags(selectedItemName, owned: true);
        }
    }

    private void ReturnCoin()
    {
        int coinNum = SaveManager.LoadCoin();
        coinNum += 200;
        SaveManager.SaveCoin(coinNum);
    }
}
