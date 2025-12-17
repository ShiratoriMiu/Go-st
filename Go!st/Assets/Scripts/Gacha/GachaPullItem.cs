using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GachaPullItem : MonoBehaviour
{
    public enum PullResultType
    {
        New,
        DuplicateColorUnlock,
        DuplicateCoin
    }

    public struct PullResult
    {
        public PullResultType type;
        public ItemColorChangeSlotColor unlockedColor; // 色解放時のみ有効
    }

    // ダブりで色解放された分の待ち行列
    private struct PendingColorChange
    {
        public GameObject icon;
        public Sprite sprite;
    }
    private List<PendingColorChange> pendingColorChanges = new List<PendingColorChange>();

    [SerializeField] Transform itemIconBase;
    [SerializeField] Animator[] graveOverAnims;
    [SerializeField] ShakeAnimation[] shakeAnimations;
    [SerializeField] GameObject titleButton;
    [SerializeField] GameObject tapText;
    [SerializeField] float offsetY;
    [SerializeField] float returnDuration = 0.5f; // 戻るアニメーションの時間
    [SerializeField] Color defaultBGColor;

    private Vector3 originalPos;

    private List<ItemData> pullResults; // 抽選されたアイテム結果
    private int pullIndex = 0;          // 次に表示するアイテムのインデックス
    private bool pullItemFlag = false;
    private bool isGraveOver = false;
    private bool isEnableAllIcon = false;
    private List<GameObject> icons;     // Hierarchy 上に配置済みのアイコンを保持

    private HashSet<string> tempOwnedItems;//仮の所持状態を保持

    // --- 追加フィールド ---
    private List<Image> pendingDuplicateIcons = new List<Image>(); // ダブりアイコンの Image リスト
    private bool duplicateSequenceRunning = false; // 多重実行防止
                                                   // --------------------
    [SerializeField] float duplicateStayTime = 0.4f; // 下で止まる時間

    private bool isNinePull = false;

    private int selectedIconIndex = -1; // 1回ガチャで選ばれたアイコン

    private bool hasDuplicateMove = false;

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
        isNinePull = (pullNum == 9);

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
            pendingDuplicateIcons.Clear(); // 前の残りがあればクリア
            for (int i = 0; i < 9; i++)
            {
                PullResult result = UpdateIconDisplay(icons[i], pullResults[i]);

                if (result.type == PullResultType.DuplicateColorUnlock)
                {
                    hasDuplicateMove = true;
                    QueueColorChange(icons[i], pullResults[i], result.unlockedColor);
                }
            }
            // 9回のときは ReturnBase 後に、ダブりがあれば追加演出を1回だけ行う（setTitleIfNoDuplicate == false）
            StartCoroutine(FinishAfterSelection(setTitleIfNoDuplicate: false));
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

        selectedIconIndex = index;

        // UpdateIconDisplay は内部で duplicate の場合 pendingDuplicateIcons に追加する
        PullResult result = UpdateIconDisplay(icon, pullResults[pullIndex]);

        if (result.type == PullResultType.DuplicateColorUnlock)
        {
            hasDuplicateMove = true;
            QueueColorChange(icon, pullResults[pullIndex], result.unlockedColor);
        }

        graveOverAnims[index].enabled = true;
        graveOverAnims[index].Play("GraveOverObake", 0, 0f);
        pullIndex++;

        if (pullIndex >= pullResults.Count)
        {
            // 非9回（選択式）のときは、ReturnBase の後にダブり演出を行い、
            // ダブりがなければ ReturnBase 後にすぐタイトル表示する (setTitleIfNoDuplicate: true)
            StartCoroutine(FinishAfterSelection(setTitleIfNoDuplicate: true));

            tapText.SetActive(false);
        }
    }

    /// <summary>
    /// アイコン表示を行い、ダブり種別を返す（かつ、ダブり時は pendingDuplicateIcons に doubleIcon を追加する）
    /// ※ doubleIcon はこのメソッド内では Show しない（演出側で制御する）
    /// </summary>
    private PullResult UpdateIconDisplay(GameObject icon, ItemData currentItem)
    {
        PullResult result = new PullResult
        {
            type = PullResultType.New,
            unlockedColor = default
        };

        Image iconBG = icon.GetComponent<Image>();
        var images = icon.GetComponentsInChildren<Image>(true);

        Image doubleIcon = null;
        Image img = null;

        foreach (var image in images)
        {
            if (image.gameObject == icon) continue; // 親は除外

            if (image.CompareTag("DoubleIcon"))
                doubleIcon = image;
            else
                img = image;
        }

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

        iconBG.color = defaultBGColor;

        bool alreadyOwned = currentItem.isOwned || tempOwnedItems.Contains(currentItem.name);

        // 所持判定と更新
        if (alreadyOwned)
        {
            // ダブり処理（色解放系もダブり扱いにする）
            if (currentItem.canColorChange && !currentItem.colorComplete)
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

                    result.type = PullResultType.DuplicateColorUnlock;
                    result.unlockedColor =
                        (ItemColorChangeSlotColor)System.Enum.Parse(
                            typeof(ItemColorChangeSlotColor),
                            newColor
                        );
                }

                SaveManager.UpdateItemFlags(currentItem.name, owned: true);
                return result;
            }
            else
            {
                // 完全ダブり（コイン返却）
                ReturnCoin();

                hasDuplicateMove = true;

                if (doubleIcon != null && !pendingDuplicateIcons.Contains(doubleIcon))
                {
                    doubleIcon.gameObject.SetActive(false);
                    pendingDuplicateIcons.Add(doubleIcon);
                }

                SaveManager.UpdateItemFlags(currentItem.name, owned: true);

                result.type = PullResultType.DuplicateCoin;
                return result;
            }
        }
        else
        {
            // 初取得扱い
            tempOwnedItems.Add(currentItem.name);

            // デフォルトカラーを自動解放
            SaveManager.SaveUnlockedColor(
                currentItem.name,
                currentItem.defaultColor.ToString()
            );

            SaveManager.UpdateItemFlags(currentItem.name, owned: true);

            result.type = PullResultType.New;
            return result;
        }
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

        if (!hasDuplicateMove)
        {
            titleButton.SetActive(true);
        }

        SetIconsActive(true);
        SoundManager.Instance.PlaySE("AppearGhostSE");
        for (int i = 0; i < graveOverAnims.Length; i++)
        {
            graveOverAnims[i].enabled = true;
            graveOverAnims[i].Play("GraveOverObake", 0, 0f);
        }
        isGraveOver = true;
    }

    void GraveIn()
    {
        for (int i = 0; i < graveOverAnims.Length; i++)
        {
            graveOverAnims[i].Play("GraveInObake", 0, 0f);
        }
        isGraveOver = false;
    }

    public void StartShakes()
    {
        SoundManager.Instance.PlaySE("GroundShakeSE");
        for (int i = 0; i < shakeAnimations.Length; i++)
        {
            shakeAnimations[i].StartShake();
        }
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

    // --- 共通移動コルーチン（再利用） ---
    private IEnumerator MoveBaseSmooth(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            itemIconBase.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        itemIconBase.localPosition = to;
    }

    /// <summary>
    /// ReturnBase 完了後に、ダブりがあれば1回だけ追加の下→表示→上 を行う。
    /// setTitleIfNoDuplicate == true のとき（通常の選択式）:
    ///   - ダブりがなければ ReturnBase 後に titleButton を出す
    ///   - ダブりがあれば ReturnBase 後に追加演出を行い、完了後 titleButton を出す
    /// setTitleIfNoDuplicate == false のとき（9回表示時）:
    ///   - ダブりがなければ何もしない（従来のまま）
    ///   - ダブりがあれば追加演出を行い、完了後 titleButton を出す
    /// </summary>
    private IEnumerator FinishAfterSelection(bool setTitleIfNoDuplicate)
    {
        // ★ まず必ず上に戻す（9回対応）
        if (itemIconBase.localPosition != originalPos)
        {
            yield return StartCoroutine(
                MoveBaseSmooth(itemIconBase.localPosition, originalPos, returnDuration)
            );
        }

        if (hasDuplicateMove)
        {
            // 上に表示された状態で少し待つ
            if (isNinePull) yield return new WaitForSeconds(duplicateStayTime + 0.7f);

            if (!duplicateSequenceRunning)
            {
                duplicateSequenceRunning = true;
                yield return StartCoroutine(DuplicateMoveSequenceForPending());
                duplicateSequenceRunning = false;
            }

            pendingDuplicateIcons.Clear();
        }
        else if (setTitleIfNoDuplicate)
        {
            titleButton.SetActive(true);
        }
    }

    /// <summary>
    /// pendingDuplicateIcons に入っているアイコン群を表示する一連の演出（itemIconBase は共通）
    /// </summary>
    private IEnumerator DuplicateMoveSequenceForPending()
    {
        Vector3 downPos = originalPos + new Vector3(0, -offsetY, 0);

        // 上に表示された状態で少し待つ
        yield return new WaitForSeconds(duplicateStayTime);

        if (isNinePull)
        {
            GraveIn();
        }
        else
        {
            graveOverAnims[selectedIconIndex].Play("GraveInObake", 0, 0f);
        }
        
        // 下に移動
        yield return StartCoroutine(
            MoveBaseSmooth(itemIconBase.localPosition, downPos, returnDuration * 0.6f)
        );

        // 全部消える
        SetIconsActive(false);

        yield return new WaitForEndOfFrame();

        //アイコン差し替え
        foreach (var change in pendingColorChanges)
        {
            if (change.icon != null && change.sprite != null)
            {
                ChangeIconForColorUnlockImmediate(change.icon, change.sprite); // 即時差し替え
            }
        }

        // ダブりアイコンだけ表示
        foreach (var img in pendingDuplicateIcons)
        {
            if (img != null)
            {
                img.gameObject.SetActive(true);
                img.GetComponent<IconSpriteHolderGhost>().ChangeCoinGhostImage();
            }
        }

        // 少し待つ
        yield return new WaitForSeconds(duplicateStayTime);

        if (isNinePull)
        {
            GraveOver();
        }
        else
        {
            graveOverAnims[selectedIconIndex].Play("GraveOverObake", 0, 0f);
        }

        // アイコン復活
        SetIconsActive(true);

        // 上に戻る
        yield return StartCoroutine(
            MoveBaseSmooth(downPos, originalPos, returnDuration)
        );

        hasDuplicateMove = false;
        pendingDuplicateIcons.Clear();

        titleButton.SetActive(true);
        tapText.SetActive(false);
    }

    // 即時適用（必要なら使える）
    private void ChangeIconForColorUnlockImmediate(GameObject icon, Sprite sprite)
    {
        Image img = icon
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i =>
                i.gameObject != icon &&
                !i.CompareTag("DoubleIcon"));

        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
        }
    }

    private void QueueColorChange(GameObject icon, ItemData item, ItemColorChangeSlotColor unlockedColor)
    {
        ItemNameSlot itemName =
            (ItemNameSlot)System.Enum.Parse(
                typeof(ItemNameSlot),
                item.name
            );

        Sprite sprite = ItemColorDatabase.Instance.GetSprite(itemName, unlockedColor);
        if (sprite != null)
        {
            pendingColorChanges.Add(new PendingColorChange { icon = icon, sprite = sprite });
        }
    }
}