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
    [SerializeField] PlayerController[] players;

    // Start is called before the first frame update
    void Start()
    {
        titleButton.onClick.AddListener(ToTitle);
        restartButton.onClick.AddListener(Restart);
        noSkillCoolTimeToggle.onValueChanged.AddListener(OnToggleChanged);
        timelimitInputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void ToTitle()
    {
        gameManager.ToTitle();
    }

    void Restart()
    {
        gameManager.Restart();
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            foreach(var player in players)
            {
                //player.SetSkillCooldownTime(0);
            }
        }
        else
        {
            foreach (var player in players)
            {
                //player.ResetSetSkillCooldownTime();
            }
        }
    }

    void OnInputValueChanged(string newText)
    {
        gameManager.SetMaxTimeLimit(float.Parse(newText));
    }
}
