using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] Text nameText;
    //[SerializeField] Text rankText;
    [SerializeField] Text bestScoreText;
    [SerializeField] GameObject changePlayerIconScrollViewBase;
    [SerializeField] RectTransform changePlayerIconScrollView;

    [SerializeField] private FirebaseController firebaseController;

    [SerializeField] private Text colorNumText;
    [SerializeField] private Text skinItemNumText;
    [SerializeField] private Text makeUpNumText;

    [SerializeField] private InputFieldValidator inputFieldValidator;
    [SerializeField] private GameManager gameManager;

    private int colorNum;
    private int skinItemNum;
    private int makeUpNum;

    private int colorNumMax;
    private int skinItemNumMax;
    private int makeUpNumMax;

    private async void OnEnable()
    {
        string name = LoginController.Instance.GetUserName();
        nameText.text = $"Name : {name}";

        var bestScore = await firebaseController.OnMyRankingButtonPressed();

        //rankText.text = $"Rank : {bestScore.rank.ToString()}";

        bestScoreText.text = $"{bestScore.score.ToString()}";

        OwnedItemNum();
    }

    public void ShowChangePlayerIconScrollView()
    {
        changePlayerIconScrollViewBase.SetActive(true);

        // ‰ŠúƒXƒP[ƒ‹‚ð0‚É
        changePlayerIconScrollView.localScale = Vector3.zero;

        // 0 ¨ 1 ‚ÉŠg‘åi0.25•bj
        changePlayerIconScrollView.DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack); // ­‚µ’e‚ÞŠ´‚¶‚É
    }

    public void CloseChangePlayerIconScrollView()
    {
        // 1 ¨ 0 ‚Ék¬i0.2•bj
        changePlayerIconScrollView.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                changePlayerIconScrollViewBase.SetActive(false);
            });
    }

    private void OwnedItemNum()
    {
        colorNum = 0;
        skinItemNum = 0;
        makeUpNum = 0;

        colorNumMax = 0;
        skinItemNumMax = 0;
        makeUpNumMax = 0;

        List<ItemData> allItemData = SaveManager.AllItems();

        foreach (ItemData item in allItemData)
        {
            if (item.itemStyle == ItemStyle.SkinItem)
            {
                if(item.isOwned)skinItemNum++;
                skinItemNumMax++;
            }
            else if (item.itemStyle == ItemStyle.SkinColor)
            {
                if (item.isOwned) colorNum++;
                colorNumMax++;
            }
            else if (item.itemStyle == ItemStyle.MakeUp)
            {
                if (item.isOwned) makeUpNum++;
                makeUpNumMax++;
            }
        }

        colorNumText.text = $"{colorNum} / {colorNumMax}";
        skinItemNumText.text = $"{skinItemNum} / {skinItemNumMax}";
        makeUpNumText.text = $"{makeUpNum} / {makeUpNumMax}";
    }

    public void CloseProfileUI()
    {
        if (inputFieldValidator.OnClick_SaveName())
        {
            gameManager.ToTitle(false);
        }
    }
}
