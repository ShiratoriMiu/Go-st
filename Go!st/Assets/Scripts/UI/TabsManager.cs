using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabsManager : MonoBehaviour
{
    [SerializeField] GameObject[] tabs;

    public void SwitchToTab(int _tabID)
    {
        foreach(GameObject go in tabs)
        {
            go.SetActive(false);
        }
        tabs[_tabID].SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
