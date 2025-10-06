using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] Text nameText;
    //[SerializeField] Text rankText;
    [SerializeField] Text bestScoreText;
    [SerializeField] GameObject changePlayerIconScrollView;

    [SerializeField] private FirebaseController firebaseController;

    [SerializeField] private Text colorNumText;
    [SerializeField] private Text skinItemNumText;
    [SerializeField] private Text makeUpNumText;

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
        changePlayerIconScrollView.SetActive(true);
    }

    public void CloseChangePlayerIconScrollView()
    {
        changePlayerIconScrollView.SetActive(false);
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
}
