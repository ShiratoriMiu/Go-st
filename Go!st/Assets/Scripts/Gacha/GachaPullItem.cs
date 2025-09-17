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

    [SerializeField] Sprite dummySprite;          // ���I���A�C�R���̉摜
    [SerializeField] Color dummyColor = Color.gray; // ���I�����̐F

    [SerializeField] Animator[] graveOverAnims;


    private List<ItemData> pullResults; // ���I���ꂽ�A�C�e������
    private int pullIndex = 0;          // ���ɕ\������A�C�e���̃C���f�b�N�X

    private bool pullItemFlag = false;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(WaitForInitializeAndPull());
    }

    private IEnumerator WaitForInitializeAndPull()
    {
        // LoadItemData �����݂��ď�������������܂őҋ@
        yield return new WaitUntil(() =>
            LoadItemData.Instance != null && LoadItemData.Instance.IsInitialized);

        Debug.Log("�A�C�e���f�[�^���������� �� �K�`���J�n");
        //PullItem();
    }

    public void PullItem()
    {
        if (pullItemFlag) return;

        // �A�C�e�����X�g����K�`�������擾
        List<ItemData> allItems = SaveManager.AllItems();
        List<string> gachaItems = SaveManager.GachaItems();

        List<ItemData> gachaItemDatas = allItems
            .Where(x => gachaItems.Contains(x.name))
            .ToList();

        int pullNum = GachaController.Instance.pullNum;

        // pullNum ���I���ĕۑ�
        pullResults = new List<ItemData>();
        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            pullResults.Add(gachaItemDatas[randIndex]);
        }

        // -------------------------
        // ���򏈗�
        // -------------------------
        if (pullNum == 9)
        {
            // �S���\������
            for (int i = 0; i < 9; i++)
            {
                ItemData currentItem = pullResults[i];

                GameObject icon = Instantiate(itemIconPrefab, itemIconBase);
                Image iconBG = icon.GetComponent<Image>();
                Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
                Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

                // �A�C�e���\��
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

                // ��������ƍX�V
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
            // 9�_�~�[�𐶐����đI�����ɂ���
            for (int i = 0; i < 9; i++)
            {
                GameObject icon = Instantiate(itemIconPrefab, itemIconBase);
                Image iconBG = icon.GetComponent<Image>();
                Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
                Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

                // �_�~�[�\��
                iconBG.color = dummyColor;
                img.sprite = dummySprite;
                txt.text = "";

                // �{�^���������Ɂu�����ɃA�C�e�����o���v������o�^
                int index = i;
                Button btn = icon.GetComponent<Button>();
                btn.onClick.AddListener(() => OnIconClick(icon, index));
            }
        }

        pullItemFlag = true;
    }


    private void OnIconClick(GameObject icon, int index)
    {
        // �������ʂ��c���ĂȂ���Ή������Ȃ�
        if (pullIndex >= pullResults.Count) return;

        ItemData currentItem = pullResults[pullIndex];
        pullIndex++; // ���̃A�C�e���ɐi��

        Image iconBG = icon.GetComponent<Image>();
        Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
        Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

        // �A�C�e���\��
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

        // ��������ƍX�V
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

        Debug.Log($"�A�C�e���\��: {currentItem.name}");
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
