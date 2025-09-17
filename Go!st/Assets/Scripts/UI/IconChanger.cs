using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconChanger : MonoBehaviour
{
    [SerializeField] private Image playerIconImage;
    [SerializeField] private Image playerIconBGImage;

    private void Awake()
    {
        UpdatePlayerIcon();
    }

    private void OnEnable()
    {
        UpdatePlayerIcon();
    }

    public void UpdatePlayerIcon()
    {
        List<PlayerIconData> equippedPlayerIcons = SaveManager.LoadEquippedPlayerIcons();
        if (equippedPlayerIcons == null) return;

        foreach (var playerIcon in equippedPlayerIcons)
        {
            if (playerIcon.style == PlayerIconStyle.BackGround)
                playerIconBGImage.sprite = Resources.Load<Sprite>("PlayerIcon/" + playerIcon.name);
            else if (playerIcon.style == PlayerIconStyle.Chara)
                playerIconImage.sprite = Resources.Load<Sprite>("PlayerIcon/" + playerIcon.name);
        }
    }
}
