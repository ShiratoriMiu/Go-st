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

    [SerializeField] Sprite dummySprite;          // 未選択アイコンの画像
    [SerializeField] Color dummyColor = Color.gray; // 未選択時の色

    [SerializeField] Animator[] graveOverAnims;


    private List<ItemData> pullResults; // 抽選されたアイテム結果
    private int pullIndex = 0;          // 次に表示するアイテムのインデックス

    private bool pullItemFlag = false;

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
        //PullItem();
    }

    public void PullItem()
    {
        if (pullItemFlag) return;

        // アイテムリストからガチャ候補を取得
        List<ItemData> allItems = SaveManager.AllItems();
        List<string> gachaItems = SaveManager.GachaItems();

        List<ItemData> gachaItemDatas = allItems
            .Where(x => gachaItems.Contains(x.name))
            .ToList();

        int pullNum = GachaController.Instance.pullNum;

        // pullNum 個抽選して保存
        pullResults = new List<ItemData>();
        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            pullResults.Add(gachaItemDatas[randIndex]);
        }

        // -------------------------
        // 分岐処理
        // -------------------------
        if (pullNum == 9)
        {
            // 全部表示する
            for (int i = 0; i < 9; i++)
            {
                ItemData currentItem = pullResults[i];

                GameObject icon = Instantiate(itemIconPrefab, itemIconBase);
                Image iconBG = icon.GetComponent<Image>();
                Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
                Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

                // アイテム表示
                iconBG.color = currentItem.ToColor();
                img.sprite = !string.IsNullOrEmpty(currentItem.IconName)
                    ? Resources.Load<Sprite>($"Icon/{currentItem.IconName}")
                    : null;

                if (img.sprite == null)
                {
                    img.color = Color.white;
                    if (currentItem.IconName != "Null") txt.text = currentItem.IconName;
                }
                else
                {
                    txt.text = "";
                }

                // 所持判定と更新
                if (currentItem.isOwned)
                {
                    if (currentItem.canColorChange && !currentItem.isColorChangeOn)
                    {
                        SaveManager.UpdateItemFlags(currentItem.name, colorChangeOn: true);
                    }
                    else
                    {
                        ReturnCoin();
                    }
                }
                SaveManager.UpdateItemFlags(currentItem.name, owned: true);
            }
        }
        else
        {
            // 9個ダミーを生成して選択式にする
            for (int i = 0; i < 9; i++)
            {
                GameObject icon = Instantiate(itemIconPrefab, itemIconBase);
                Image iconBG = icon.GetComponent<Image>();
                Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
                Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

                // ダミー表示
                iconBG.color = dummyColor;
                img.sprite = dummySprite;
                txt.text = "";

                // ボタン押下時に「ここにアイテムを出す」処理を登録
                int index = i;
                Button btn = icon.GetComponent<Button>();
                btn.onClick.AddListener(() => OnIconClick(icon, index));
            }
        }

        pullItemFlag = true;
    }


    private void OnIconClick(GameObject icon, int index)
    {
        // もう結果が残ってなければ何もしない
        if (pullIndex >= pullResults.Count) return;

        ItemData currentItem = pullResults[pullIndex];
        pullIndex++; // 次のアイテムに進む

        Image iconBG = icon.GetComponent<Image>();
        Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
        Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

        // アイテム表示
        iconBG.color = currentItem.ToColor();
        img.sprite = !string.IsNullOrEmpty(currentItem.IconName)
            ? Resources.Load<Sprite>($"Icon/{currentItem.IconName}")
            : null;

        if (img.sprite == null)
        {
            img.color = Color.white;
            if (currentItem.IconName != "Null") txt.text = currentItem.IconName;
        }
        else
        {
            txt.text = "";
        }

        // 所持判定と更新
        if (currentItem.isOwned)
        {
            if (currentItem.canColorChange && !currentItem.isColorChangeOn)
            {
                SaveManager.UpdateItemFlags(currentItem.name, colorChangeOn: true);
            }
            else
            {
                ReturnCoin();
            }
        }
        SaveManager.UpdateItemFlags(currentItem.name, owned: true);

        Debug.Log($"アイテム表示: {currentItem.name}");
    }

    private void ReturnCoin()
    {
        int coinNum = SaveManager.LoadCoin();
        coinNum += 200;
        SaveManager.SaveCoin(coinNum);
    }

    public void GraveOver()
    {
        for (int i = 0; i < graveOverAnims.Count(); i++)
        {
            graveOverAnims[i].enabled = true;
        }
    }
}
