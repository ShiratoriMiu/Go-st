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

        // ���ʂ����郊�X�g
        List<string> pulledItems = new List<string>();

        // pullNum �񃉃��_���ɑI��
        for (int i = 0; i < GachaController.Instance.pullNum; i++)
        {
            int randIndex = Random.Range(0, allItems.Count);
            Debug.Log("�S�A�C�e�����X�g" + allItems.Count);
            string selectedItem = allItems[randIndex].name;
            pulledItems.Add(selectedItem);//�l�������A�C�e���̖��O��o�^

            GameObject icon = Instantiate(itemIconPrefab, itemIconBase);//���ʕ\���p�̃A�C�R���𐶐�
            //�A�C�R���̔w�i�̐F��ύX
            icon.GetComponent<Image>().color = new Color(allItems[randIndex].color[0], allItems[randIndex].color[1], allItems[randIndex].color[2], allItems[randIndex].color[3]);
            //�A�C�R���̎q��Sprite��ύX
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

        // �o��
        Debug.Log("�K�`������:");
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
