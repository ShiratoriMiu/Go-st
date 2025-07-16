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
    [SerializeField] Toggle oneHandModeToggle;
    [SerializeField] Toggle leftHandModeToggle;

    [SerializeField] PlayerManager playerManager;

    [SerializeField] GameUIPanelData leftHandModePanelData;
    [SerializeField] GameUIPanelData rightHandModePanelData;

    private PlayerController playerController;
    private PlayerSkill playerSkill;

    // Start is called before the first frame update
    void Start()
    {
        leftHandModeToggle.onValueChanged.AddListener(OnLeftHandModeToggleChanged);
        oneHandModeToggle.onValueChanged.AddListener(OnOneHandModeToggleChanged);

        playerController = playerManager.Player.GetComponent<PlayerController>();
        playerSkill = playerManager.Player.GetComponent<PlayerSkill>();
    }

    void OnLeftHandModeToggleChanged(bool isOn)
    {
        leftHandModePanelData.panelRoot.SetActive(isOn);
        rightHandModePanelData.panelRoot.SetActive(!isOn);

        if (isOn) {
            playerSkill.SetSkillButton(leftHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(leftHandModePanelData.skillGaugeImage);
        }
        else
        {
            playerSkill.SetSkillButton(rightHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(rightHandModePanelData.skillGaugeImage);
        }

        playerController.SwitchStickPos();
    }

    void OnOneHandModeToggleChanged(bool isOn)
    {
        playerSkill.isOneHand = isOn;
    }
}
