using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPopUpController : MonoBehaviour
{
    public static ShopPopUpController Instance { get; private set; }

    [SerializeField]
    private GameObject shopPop;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ä˘Ç…ë∂ç›Ç∑ÇÈèÍçáÇÕîjä¸
            return;
        }

        Instance = this;
        shopPop.SetActive(false);
    }

    public void ShowShopPop()
    {
        shopPop.SetActive(true);
    }

    public void CloseShopPop()
    {
        shopPop.SetActive(false);
    }
}
