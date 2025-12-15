using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GachaPullItem : MonoBehaviour
{
    [SerializeField] Transform itemIconBase;
    [SerializeField] Animator[] graveOverAnims;
    [SerializeField] ShakeAnimation[] shakeAnimations;
    [SerializeField] GameObject titleButton;
    [SerializeField] GameObject tapText;
    [SerializeField] float offsetY;
    [SerializeField] float returnDuration = 0.5f; // 戻るアニメーションの時間

    private Vector3 originalPos;

    private List<ItemData> pullResults; // 抽選されたアイテム結果
    private int pullIndex = 0;          // 次に表示するアイテムのインデックス
    private bool pullItemFlag = false;
    private bool isGraveOver = false;
    private bool isEnableAllIcon = false;
    private List<GameObject> icons;     // Hierarchy 上に配置済みのアイコンを保持

    private HashSet<string> tempOwnedItems;//仮の所持状態を保持

    private void Start()
    {
        titleButton.SetActive(false);
        tapText.SetActive(false);

        // すでに配置されているアイコンを取得して保持
        icons = itemIconBase.Cast<Transform>()
                            .Select(t => t.gameObject)
                            .ToList();

        // 元の位置を保持
        originalPos = itemIconBase.localPosition;

        // 最初に下げる
        itemIconBase.localPosition = originalPos + new Vector3(0, -offsetY, 0);

        // 最初は非表示にする
        SetIconsActive(false);

        // 最初はボタン無効にする
        SetIconsButtonInteractable(false);

        StartCoroutine(WaitForInitializeAndPull());
    }

    private IEnumerator WaitForInitializeAndPull()
    {
        yield return new WaitUntil(() =>
            LoadItemData.Instance != null && LoadItemData.Instance.IsInitialized);

        Debug.Log("アイテムデータ初期化完了 → ガチャ開始");
        PullItem();
    }

    void PullItem()
    {
        if (pullItemFlag) return;

        // ガチャ候補取得
        List<ItemData> allItems = SaveManager.AllItems();
        List<string> gachaItems = SaveManager.GachaItems();
        List<ItemData> gachaItemDatas = allItems
            .Where(x => gachaItems.Contains(x.name))
            .ToList();

        int pullNum = GachaController.Instance.pullNum;

        // 抽選結果を保存
        pullResults = new List<ItemData>();
        tempOwnedItems = new HashSet<string>();

        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            pullResults.Add(gachaItemDatas[randIndex]);
        }

        if (pullNum == 9)
        {
            // 全部表示する
            for (int i = 0; i < 9; i++)
            {
                UpdateIconDisplay(icons[i], pullResults[i]);
            }
            StartCoroutine(ReturnBaseSmooth());
        }
        else
        {
            SetIconsActive(true);
            // 選択式用ダミー表示
            for (int i = 0; i < 9; i++)
            {
                SetDummyIcon(icons[i]);

                // ボタン押下時に結果を表示
                int index = i;
                Button btn = icons[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnIconClick(icons[index], index));
            }
        }

        pullItemFlag = true;
    }

    private void OnIconClick(GameObject icon, int index)
    {
        if (pullIndex >= pullResults.Count) return;

        UpdateIconDisplay(icon, pullResults[pullIndex]);
        graveOverAnims[index].enabled = true;
        pullIndex++;

        if (pullIndex >= pullResults.Count)
        {
            StartCoroutine(ReturnBaseSmooth());
            titleButton.SetActive(true);
            tapText.SetActive(false);
        }
    }

    private void UpdateIconDisplay(GameObject icon, ItemData currentItem)
    {
        Image iconBG = icon.GetComponent<Image>();
        Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
        Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

        // アイテム表示
        img.sprite = !string.IsNullOrEmpty(currentItem.IconName)
            ? Resources.Load<Sprite>($"Icon/{currentItem.IconName}")
            : null;
        img.color = Color.white;

        if (img.sprite == null)
        {
            img.color = Color.white;
            if (currentItem.IconName != "Null") txt.text = currentItem.IconName;
        }
        else
        {
            txt.text = "";
            img.color = Color.white;
        }

        iconBG.color = Color.white;

        bool alreadyOwned = currentItem.isOwned || tempOwnedItems.Contains(currentItem.name);

        // 所持判定と更新
        if (alreadyOwned)
        {
            // ダブり処理
            if (currentItem.canColorChange && !currentItem.isColorChangeOn)
            {
                SaveManager.UpdateItemFlags(currentItem.name, colorChangeOn: true);

                List<string> unlocked = SaveManager.LoadUnlockedColors(currentItem.name);
                var allColors = System.Enum.GetNames(typeof(ItemColorChangeSlotColor));
                var locked = allColors.Where(c => !unlocked.Contains(c)).ToList();

                if (locked.Count > 0)
                {
                    string newColor = locked[Random.Range(0, locked.Count)];
                    SaveManager.SaveUnlockedColor(currentItem.name, newColor);
                    SaveManager.UpdateItemColorComplete(currentItem.name);
                }
            }
            else
            {
                ReturnCoin();
            }
        }
        else
        {
            // 初取得扱い
            tempOwnedItems.Add(currentItem.name);
        }
        SaveManager.UpdateItemFlags(currentItem.name, owned: true);
    }

    private void SetDummyIcon(GameObject icon)
    {
        Image iconBG = icon.GetComponent<Image>();
        Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
        Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

        iconBG.color = Color.clear;
        img.sprite = null;
        img.color = Color.clear;
        txt.text = "";
    }

    private void SetIconsActive(bool active)
    {
        foreach (var icon in icons)
        {
            icon.SetActive(active);
        }
    }

    private void ReturnCoin()
    {
        int coinNum = SaveManager.LoadCoin();
        coinNum += 200;
        SaveManager.SaveCoin(coinNum);
    }

    public void GraveOver()
    {
        if (isGraveOver) return;
        titleButton.SetActive(true);
        SetIconsActive(true);
        SoundManager.Instance.PlaySE("AppearGhostSE");
        for (int i = 0; i < graveOverAnims.Length; i++)
        {
            graveOverAnims[i].enabled = true;
        }
        isGraveOver = true;
    }

    public void StartShakes()
    {
        SoundManager.Instance.PlaySE("GroundShakeSE");
        for (int i = 0; i < shakeAnimations.Length; i++)
        {
            shakeAnimations[i].StartShake();
        }
    }

    private IEnumerator ReturnBaseSmooth()
    {
        Vector3 start = itemIconBase.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / returnDuration;
            itemIconBase.localPosition = Vector3.Lerp(start, originalPos, t);
            yield return null;
        }
        itemIconBase.localPosition = originalPos; // 最終補正
    }

    /// すべてのアイコンのボタンを押せるようにする
    public void EnableAllIconButtons()
    {
        if (isEnableAllIcon) return;
        SetIconsButtonInteractable(true);
        tapText.SetActive(true);
        isEnableAllIcon = true;
    }

    /// icons 内のボタンの interactable をまとめて切り替える
    private void SetIconsButtonInteractable(bool interactable)
    {
        foreach (var icon in icons)
        {
            var btn = icon.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }

}
