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
    [SerializeField] float offsetY;
    [SerializeField] float returnDuration = 0.5f; // �߂�A�j���[�V�����̎���

    private Vector3 originalPos;

    private List<ItemData> pullResults; // ���I���ꂽ�A�C�e������
    private int pullIndex = 0;          // ���ɕ\������A�C�e���̃C���f�b�N�X
    private bool pullItemFlag = false;
    private bool isGraveOver = false;
    private List<GameObject> icons;     // Hierarchy ��ɔz�u�ς݂̃A�C�R����ێ�

    private void Start()
    {
        titleButton.SetActive(false);

        // ���łɔz�u����Ă���A�C�R�����擾���ĕێ�
        icons = itemIconBase.Cast<Transform>()
                            .Select(t => t.gameObject)
                            .ToList();

        // ���̈ʒu��ێ�
        originalPos = itemIconBase.localPosition;

        // �ŏ��ɉ�����
        itemIconBase.localPosition = originalPos + new Vector3(0, -offsetY, 0);

        // �ŏ��͔�\���ɂ���
        SetIconsActive(false);

        StartCoroutine(WaitForInitializeAndPull());
    }

    private IEnumerator WaitForInitializeAndPull()
    {
        yield return new WaitUntil(() =>
            LoadItemData.Instance != null && LoadItemData.Instance.IsInitialized);

        Debug.Log("�A�C�e���f�[�^���������� �� �K�`���J�n");
        //PullItem();
    }

    public void PullItem()
    {
        if (pullItemFlag) return;

        // �K�`�����擾
        List<ItemData> allItems = SaveManager.AllItems();
        List<string> gachaItems = SaveManager.GachaItems();
        List<ItemData> gachaItemDatas = allItems
            .Where(x => gachaItems.Contains(x.name))
            .ToList();

        int pullNum = GachaController.Instance.pullNum;

        // ���I���ʂ�ۑ�
        pullResults = new List<ItemData>();
        for (int i = 0; i < pullNum; i++)
        {
            int randIndex = Random.Range(0, gachaItemDatas.Count);
            pullResults.Add(gachaItemDatas[randIndex]);
        }

        SetIconsActive(true);

        if (pullNum == 9)
        {
            // �S���\������
            for (int i = 0; i < 9; i++)
            {
                UpdateIconDisplay(icons[i], pullResults[i]);
            }
            StartCoroutine(ReturnBaseSmooth());
            titleButton.SetActive(true);
        }
        else
        {
            // �I�����p�_�~�[�\��
            for (int i = 0; i < 9; i++)
            {
                SetDummyIcon(icons[i]);

                // �{�^���������Ɍ��ʂ�\��
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
        pullIndex++;

        if (pullIndex >= pullResults.Count)
        {
            StartCoroutine(ReturnBaseSmooth());
            titleButton.SetActive(true);
        }
    }

    private void UpdateIconDisplay(GameObject icon, ItemData currentItem)
    {
        Image iconBG = icon.GetComponent<Image>();
        Image img = icon.GetComponentsInChildren<Image>().FirstOrDefault(x => x.gameObject != icon);
        Text txt = icon.GetComponentsInChildren<Text>().FirstOrDefault(x => x.gameObject != icon);

        // �A�C�e���\��
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
        itemIconBase.localPosition = originalPos; // �ŏI�␳
    }
}
