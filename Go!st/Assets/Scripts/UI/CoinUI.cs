using UnityEngine;

public class CoinUI : MonoBehaviour
{
    private void OnEnable()
    {
        if (CoinUIManager.Instance != null)
        {
            CoinUIManager.Instance.UpdateCoinUI();
        }
    }
}
