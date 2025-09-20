using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleGachaButton : MonoBehaviour
{
    [SerializeField] Button pull1GachaButton;
    [SerializeField] Button pull9GachaButton;

    // ”CˆÓ‚ÌURL‚ðŽw’è
    [SerializeField] private string url = "https://example.com";

    // Start is called before the first frame update
    void Start()
    {
        pull1GachaButton.onClick.AddListener(() =>
        {
            GachaController.Instance.PullGacha(1);
        });
        pull9GachaButton.onClick.AddListener(() =>
        {
            GachaController.Instance.PullGacha(9);
        });
    }

    private void OnEnable()
    {
        int pullCoinNum = GachaController.Instance.pullCoinNum;

        if (SaveManager.LoadCoin() < pullCoinNum)
        {
            pull1GachaButton.interactable = false;
            pull9GachaButton.interactable = false;
        }
        else if (SaveManager.LoadCoin() < pullCoinNum * 9)
        {
            pull1GachaButton.interactable = true;
            pull9GachaButton.interactable = false;
        }
        else
        {
            pull1GachaButton.interactable = true;
            pull9GachaButton.interactable = true;
        }
    }

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
