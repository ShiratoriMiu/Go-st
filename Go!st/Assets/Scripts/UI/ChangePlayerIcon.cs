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
        //�f�t�H���g���Z�b�g
        PlayerIconData playerIcon = new PlayerIconData(defaultPlayerIcon.name,PlayerIconStyle.Chara);
        SaveManager.SaveOwnedPlayerIcon(playerIcon);
        PlayerIconData playerIconBG = new PlayerIconData(defaultPlayerIconBG.name, PlayerIconStyle.BackGround);
        SaveManager.SaveOwnedPlayerIcon(playerIconBG);
    }

    private void OnEnable()
    {
        // ScrollView �̒��g�����ׂč폜
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
            Debug.Log("�����A�C�R��������܂���");
            return;
        }

        foreach (var playerIcon in ownedIcons)
        {
            GameObject icon = null;
            if (playerIcon.style == PlayerIconStyle.Chara) icon = Instantiate(playerIconPrefab, changePlayerIconScrollViewBase);
            else if(playerIcon.style == PlayerIconStyle.BackGround) icon = Instantiate(playerIconPrefab, changePlayerIconBGScrollViewBase);


            // �q�I�u�W�F�N�g�� Image ���擾�i���[�g�͖����j
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
                Debug.LogWarning($"{playerIcon} �̎q Image ��������܂���ł���");
            }

            // Button �̐ݒ�
            Button btn = icon.GetComponent<Button>();
            if (btn != null)
            {
                string capturedIconName = playerIcon.name; // ���[�v�ϐ����L���v�`��
                btn.onClick.AddListener(() => ChangeIcon(playerIcon));
            }
        }
    }

    async void ChangeIcon(PlayerIconData _playerIcon)
    {
        // ���[�J���ۑ�
        SaveManager.SaveEquippedPlayerIcon(_playerIcon);

        iconChanger.UpdatePlayerIcon();

        await firebaseController.EquipPlayerIconAsync(_playerIcon); 
    }
}
