using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePlayerIcon : MonoBehaviour
{
    [SerializeField] GameObject playerIconPrefab;

    [SerializeField] Transform changePlayerIconScrollViewBase;
    [SerializeField] Transform changePlayerIconBGScrollViewBase;

    [SerializeField] Sprite defaultPlayerIcon;
    [SerializeField] Sprite defaultPlayerIconBG;

    [SerializeField] IconChanger iconChanger;

    [SerializeField] FirebaseController firebaseController;

    private void Awake()
    {
        //デフォルトをセット
        PlayerIconData playerIcon = new PlayerIconData(defaultPlayerIcon.name,PlayerIconStyle.Chara);
        SaveManager.SaveOwnedPlayerIcon(playerIcon);
        PlayerIconData playerIconBG = new PlayerIconData(defaultPlayerIconBG.name, PlayerIconStyle.BackGround);
        SaveManager.SaveOwnedPlayerIcon(playerIconBG);
    }

    private void OnEnable()
    {
        // ScrollView の中身をすべて削除
        foreach (Transform child in changePlayerIconScrollViewBase)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in changePlayerIconBGScrollViewBase)
        {
            Destroy(child.gameObject);
        }

        var ownedIcons = SaveManager.LoadOwnedPlayerIcons();

        if (ownedIcons == null || ownedIcons.Count == 0)
        {
            Debug.Log("所持アイコンがありません");
            return;
        }

        foreach (var playerIcon in ownedIcons)
        {
            GameObject icon = null;
            if (playerIcon.style == PlayerIconStyle.Chara) icon = Instantiate(playerIconPrefab, changePlayerIconScrollViewBase);
            else if(playerIcon.style == PlayerIconStyle.BackGround) icon = Instantiate(playerIconPrefab, changePlayerIconBGScrollViewBase);


            // 子オブジェクトの Image を取得（ルートは無視）
            Image iconImage = null;
            Image[] images = icon.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != icon)
                {
                    iconImage = img;
                    break;
                }
            }

            if (iconImage != null)
            {
                iconImage.sprite = Resources.Load<Sprite>("PlayerIcon/" + playerIcon.name);
            }
            else
            {
                Debug.LogWarning($"{playerIcon} の子 Image が見つかりませんでした");
            }

            // Button の設定
            Button btn = icon.GetComponent<Button>();
            if (btn != null)
            {
                string capturedIconName = playerIcon.name; // ループ変数をキャプチャ
                btn.onClick.AddListener(() => ChangeIcon(playerIcon));
            }
        }
    }

    async void ChangeIcon(PlayerIconData _playerIcon)
    {
        // ローカル保存
        SaveManager.SaveEquippedPlayerIcon(_playerIcon);

        iconChanger.UpdatePlayerIcon();

        await firebaseController.EquipPlayerIconAsync(_playerIcon); 
    }
}
