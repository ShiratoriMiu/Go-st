using Google.MiniJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameUISettingManager : MonoBehaviour
{
    [SerializeField] GameObject GameUISettingSave;

    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Toggle toggleA;
    [SerializeField] private Toggle toggleB;
    [SerializeField] private Button saveButton;

    [SerializeField] GameUIPanelData leftHandModePanelData;
    [SerializeField] GameUIPanelData rightHandModePanelData;

    [SerializeField] PlayerManager playerManager;

    private PlayerController playerController;
    private PlayerSkill playerSkill;

    // 🔹 前回のトグル状態を記録する変数
    private Toggle lastActiveToggle;

    [SerializeField] GameManager gameManager;

    [SerializeField] GameObject SettingLeftHandModeUI;
    [SerializeField] GameObject SettingRightHandModeUI;

    // 🔹 保存キー
    private const string TOGGLE_SAVE_KEY = "UI_MODE"; // "Left" or "Right"

    private void Awake()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        
        playerController = playerManager.Player.GetComponent<PlayerController>();
        playerSkill = playerManager.Player.GetComponent<PlayerSkill>();

        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    OnToggleOn(toggle);
                }
            });
        }
    }

    private void Start()
    {
        // 🔹 UIがアクティブ化されたタイミングでトグル状態を反映
        LoadToggleState();

        // 🔹 最後にアクティブなトグルを記録
        lastActiveToggle = toggleGroup.ActiveToggles().FirstOrDefault();
        Debug.Log($"初期状態のトグル: {lastActiveToggle?.name ?? "なし"}");
    }


    private void OnSaveButtonClicked()
    {
        var activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();

        if (activeToggle == null)
        {
            Debug.Log("どのトグルもONではありません。");
            return;
        }

        if (activeToggle == lastActiveToggle)
        {
            Debug.Log("トグル状態に変更なし。アクションはスキップします。");
            gameManager.ToTitle(true);
            return;
        }

        if (activeToggle == toggleA)
        {
            Debug.Log("左手モードに変更");
            leftHandModePanelData.panelRoot.SetActive(true);
            rightHandModePanelData.panelRoot.SetActive(false);

            playerSkill.SetSkillButton(leftHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(leftHandModePanelData.skillGaugeImage, leftHandModePanelData.levelUpGaugeImage, leftHandModePanelData.skillIconAnim);
            playerController.SwitchStickPos();

            // 🔹 保存
            PlayerPrefs.SetString(TOGGLE_SAVE_KEY, "Left");
        }
        else if (activeToggle == toggleB)
        {
            Debug.Log("右手モードに変更");
            leftHandModePanelData.panelRoot.SetActive(false);
            rightHandModePanelData.panelRoot.SetActive(true);

            playerSkill.SetSkillButton(rightHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(rightHandModePanelData.skillGaugeImage, rightHandModePanelData.levelUpGaugeImage, rightHandModePanelData.skillIconAnim);
            playerController.SwitchStickPos();

            // 🔹 保存
            PlayerPrefs.SetString(TOGGLE_SAVE_KEY, "Right");
        }

        // 変更を保存
        PlayerPrefs.Save();

        lastActiveToggle = activeToggle;
        gameManager.ToTitle(true);
    }

    public void OpenGameUISettingSave()
    {
        GameUISettingSave.SetActive(true);
    }

    public void CloseGameUISettingSave()
    {
        GameUISettingSave.SetActive(false);
    }

    void OnToggleOn(Toggle toggle)
    {
        if (toggle == toggleA)
        {
            SettingLeftHandModeUI.SetActive(true);
            SettingRightHandModeUI.SetActive(false);
        }
        else if (toggle == toggleB)
        {
            SettingLeftHandModeUI.SetActive(false);
            SettingRightHandModeUI.SetActive(true);
        }
    }

    // 🔹 保存されたトグル状態をロード
    private void LoadToggleState()
    {
        string savedMode = PlayerPrefs.GetString(TOGGLE_SAVE_KEY, "Left"); // デフォルトは左手モード
        Debug.Log($"保存されたモード: {savedMode}");

        if (savedMode == "Left")
        {
            toggleA.isOn = true;
            toggleB.isOn = false;

            SettingLeftHandModeUI.SetActive(true);
            SettingRightHandModeUI.SetActive(false);

            leftHandModePanelData.panelRoot.SetActive(true);
            rightHandModePanelData.panelRoot.SetActive(false);

            playerSkill.SetSkillButton(leftHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(leftHandModePanelData.skillGaugeImage, leftHandModePanelData.levelUpGaugeImage, leftHandModePanelData.skillIconAnim);

            // 保存
            PlayerPrefs.SetString(TOGGLE_SAVE_KEY, "Left");
        }
        else
        {
            toggleA.isOn = false;
            toggleB.isOn = true;

            SettingLeftHandModeUI.SetActive(false);
            SettingRightHandModeUI.SetActive(true);

            leftHandModePanelData.panelRoot.SetActive(false);
            rightHandModePanelData.panelRoot.SetActive(true);

            playerSkill.SetSkillButton(rightHandModePanelData.skillButton);
            playerController.SetSkillGaugeImage(rightHandModePanelData.skillGaugeImage, rightHandModePanelData.levelUpGaugeImage, rightHandModePanelData.skillIconAnim);
            playerController.SwitchStickPos();

            // 保存
            PlayerPrefs.SetString(TOGGLE_SAVE_KEY, "Right");
        }

        // 現在のトグルを記録
        lastActiveToggle = toggleGroup.ActiveToggles().FirstOrDefault();
    }
}
