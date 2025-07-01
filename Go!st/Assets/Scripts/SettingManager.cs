using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameUIPanelData
{
    public GameObject panelRoot;
    public Button skillButton;
    public RectTransform skillGaugeImage; 
}

public class SettingManager : MonoBehaviour
{
    [SerializeField] Toggle leftHandModeToggle;

    [SerializeField] PlayerManager playerManager;

    [SerializeField] GameUIPanelData leftHandModePanelData;
    [SerializeField] GameUIPanelData rightHandModePanelData;

    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        leftHandModeToggle.onValueChanged.AddListener(OnToggleChanged);

        playerController = playerManager.Player.GetComponent<PlayerController>();
    }

    void OnToggleChanged(bool isOn)
    {
        leftHandModePanelData.panelRoot.SetActive(isOn);
        rightHandModePanelData.panelRoot.SetActive(!isOn);

        if (isOn) {
            playerController.SetSkillChargeImage(leftHandModePanelData.skillGaugeImage);
            playerController.SetSkillButton(leftHandModePanelData.skillButton);
        }
        else
        {
            playerController.SetSkillChargeImage(rightHandModePanelData.skillGaugeImage);
            playerController.SetSkillButton(rightHandModePanelData.skillButton);
        }
    }
}
