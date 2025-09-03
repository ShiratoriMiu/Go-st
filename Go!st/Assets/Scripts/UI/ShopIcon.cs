using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopIcon : MonoBehaviour
{
   Button shopButton;

    // Start is called before the first frame update
    void Start()
    {
        shopButton = this.GetComponent<Button>();

        shopButton.onClick.AddListener(() =>
        {
            ShopPopUpController.Instance.ShowShopPop();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
