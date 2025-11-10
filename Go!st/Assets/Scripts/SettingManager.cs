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
    [SerializeField] Toggle oneHandModeToggle;
    private PlayerSkill playerSkill;

    // Start is called before the first frame update
    void Start()
    {
        oneHandModeToggle.onValueChanged.AddListener(OnOneHandModeToggleChanged);
    }

    void OnOneHandModeToggleChanged(bool isOn)
    {
        playerSkill.isOneHand = isOn;
    }
}
