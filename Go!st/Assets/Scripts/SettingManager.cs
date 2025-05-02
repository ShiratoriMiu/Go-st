using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [SerializeField] Toggle noSkillCoolTimeToggle;
    [SerializeField] InputField timelimitInputField;
    [SerializeField] Button titleButton;
    [SerializeField] Button restartButton;
    [SerializeField] GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        titleButton.onClick.AddListener(ToTitle);
        restartButton.onClick.AddListener(Restart);
        noSkillCoolTimeToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void ToTitle()
    {
        gameManager.RankingToTitle();
    }

    void Restart()
    {
        gameManager.Restart();
    }

    void OnToggleChanged(bool isOn)
    {
        int value = isOn ? 1 : 0;
    }
}
