using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GlowFrame : MonoBehaviour
{
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        img.DOFade(0.9f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .From(0.2f)
            .SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        img.DOKill();
    }
}