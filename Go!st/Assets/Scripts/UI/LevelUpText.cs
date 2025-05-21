using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelUpText : MonoBehaviour
{
    [SerializeField] float moveHeight = 0.5f;
    [SerializeField] float moveDuration = 0.5f;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] RectTransform text;

    // Start is called before the first frame update
    void Start()
    {
        FadeInOut();
    }

    void FadeInOut()
    {
        canvasGroup.alpha = 0.0f;
        // フェードイン → フェードアウト
        canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                canvasGroup.DOFade(0f, fadeDuration)
                        .SetEase(Ease.InOutQuad)
                        .SetDelay(0.5f); // 表示時間
            });

        text.DOAnchorPosY(text.position.y + moveHeight, moveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {

                });
    }
}