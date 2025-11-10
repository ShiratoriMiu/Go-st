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
    public RectTransform levelUpGaugeImage;
    public Animator skillIconAnim;
}

public class SettingManager : MonoBehaviour
{
    [SerializeField] private Toggle oneHandModeToggle;
    [SerializeField] private PlayerManager playerManager;
    private PlayerSkill playerSkill;

    // PlayerPrefs 保存キー
    private const string ONE_HAND_KEY = "OneHandMode"; // 0 = OFF, 1 = ON

    private void Start()
    {
        playerSkill = playerManager.Player.GetComponent<PlayerSkill>();

        // 🔹 保存値をロード
        bool savedValue = PlayerPrefs.GetInt(ONE_HAND_KEY, 0) == 1;

        // 🔹 トグルに反映（非アクティブでもOK）
        oneHandModeToggle.isOn = savedValue;

        // 🔹 PlayerSkill に直接反映
        playerSkill.isOneHand = savedValue;

        // 🔹 トグル変更時の処理を登録
        oneHandModeToggle.onValueChanged.AddListener(OnOneHandModeToggleChanged);
    }

    private void OnOneHandModeToggleChanged(bool isOn)
    {
        // 🔹 PlayerSkill に反映
        playerSkill.isOneHand = isOn;

        // 🔹 保存
        PlayerPrefs.SetInt(ONE_HAND_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"OneHandMode を保存しました: {isOn}");
    }
}
