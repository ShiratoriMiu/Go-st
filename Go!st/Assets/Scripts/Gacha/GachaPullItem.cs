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
        // LoadItemData �����݂��ď�������������܂őҋ@
        yield return new WaitUntil(() =>
            LoadItemData.Instance != null && LoadItemData.Instance.IsInitialized);

        Debug.Log("�A�C�e���f�[�^���������� �� �K�`���J�n");
        PullItem();
    }

    void PullItem()
    {
        // �A�C�e�������X�g���擾
        List<ItemData> allItems = SaveManager.AllItems();

        // �K�`���ŏo��A�C�e�������擾
        List<string> gachaItems = SaveManager.GachaItems();

        // �S�ẴA�C�e������K�`���ŏo��A�C�e���݂̂ɍi��
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

        // pullNum �񃉃��_���ɑI��
        for (int i = 0; i < GachaController.Instance.pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            string selectedItemName = gachaItemDatas[randIndex].name;

            // ����ŐV�̏�Ԃ��擾
            ItemData currentItem = SaveManager.AllItems().First(x => x.name == selectedItemName);

            // �A�C�R������
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

            // �����ς݃A�C�e���̏���
            if (currentItem.isOwned)
            {
                if (currentItem.canColorChange)
                {
                    // �ŐV��ԂŐF�������
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

            // �����t���O���X�V
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
