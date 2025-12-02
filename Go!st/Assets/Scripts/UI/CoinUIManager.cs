using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager Instance { get; private set; }

    [SerializeField] private Text[] coinTexts; // •¡” Text ‚ð“o˜^
    private int displayedCoin = 0;

    static bool onRuntimeMethodLoad = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnRuntimeMethodLoad()
    {
        onRuntimeMethodLoad = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (onRuntimeMethodLoad)
        {
            displayedCoin = SaveManager.LoadCoin();
            foreach (var coin in coinTexts)
            {
                coin.text = displayedCoin.ToString();
            }
            onRuntimeMethodLoad = false;
        }
    }

    // Še CoinUI ‚©‚çŒÄ‚Ô
    public void UpdateCoinUI()
    {
        int targetCoin = SaveManager.LoadCoin();
        if (targetCoin == displayedCoin) return;

        DOTween.To(() => displayedCoin, x =>
        {
            displayedCoin = x;
            foreach (var t in coinTexts)
            {
                t.text = displayedCoin.ToString();
                t.transform.DOKill();
                t.transform.localScale = Vector3.one;
                t.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack)
                    .OnComplete(() => t.transform.DOScale(1f, 0.1f));
            }
        }, targetCoin, 0.8f).SetEase(Ease.OutCubic);
    }
}
