using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class TabGroup
{
    public GameObject[] tabs;
}

public class TabsManager : MonoBehaviour
{
    [SerializeField] List<TabGroup> tabList = new List<TabGroup>();

    public void SwitchToTab(int _tabID)
    {
        // �S�Ẵ^�u���\��
        foreach (TabGroup group in tabList)
        {
            foreach (GameObject tab in group.tabs)
            {
                tab.SetActive(false);
            }
        }

        // �Ώۃ^�u��\��
        foreach (GameObject tab in tabList[_tabID].tabs)
        {
            tab.SetActive(true);
        }
    }

    void Start()
    {
        SwitchToTab(0);
    }
}