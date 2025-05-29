using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelUpText : MonoBehaviour
{
    [SerializeField] Transform target;         // Textなどの子オブジェクト
    [SerializeField] CanvasGroup canvasGroup;  // フェード用

    [SerializeField] float moveDistance = 1f;  // 上に動く距離（ローカルY方向）
    [SerializeField] float fadeInDuration = 0.5f;
    [SerializeField] float stayDuration = 1.0f;
    [SerializeField] float fadeOutDuration = 0.5f;

    private Camera mainCamera;
    private Vector3 localDefaultPos;
    private Sequence animationSequence;

    void Start()
    {
        mainCamera = Camera.main;

        // 子オブジェクトのローカル初期位置を記録
        localDefaultPos = target.localPosition;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // カメラの方向を向く（ワールド空間の forward に合わせる）
            target.forward = mainCamera.transform.forward;
        }
    }

    public void PlayAnimation()
    {
        // すでに再生中なら止める
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        target.gameObject.SetActive(true);
        canvasGroup.alpha = 0;

        // 開始位置：親のローカル空間で下方向にオフセット
        Vector3 startLocalPos = localDefaultPos - new Vector3(0, moveDistance, 0);
        target.localPosition = startLocalPos;

        // アニメーション
        animationSequence = DOTween.Sequence();
        animationSequence.Append(canvasGroup.DOFade(1, fadeInDuration));
        animationSequence.Join(target.DOLocalMove(localDefaultPos, fadeInDuration).SetEase(Ease.OutCubic));
        animationSequence.AppendInterval(stayDuration);
        animationSequence.Append(canvasGroup.DOFade(0, fadeOutDuration));
        animationSequence.OnComplete(() =>
        {
            target.gameObject.SetActive(false);
        });
    }
}
