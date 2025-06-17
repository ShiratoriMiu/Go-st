using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillChargeEffect : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    private bool isRotating = false;

    private RectTransform rectTransform;

    // アニメーション設定
    [SerializeField] float appearDuration = 0.3f;
    [SerializeField] float disappearDuration = 0.2f;

    private Vector3 startScale = Vector3.zero;
    private Vector3 endScale;

    [SerializeField] GameManager gameManager;

    private GameObject oldPlayer;//保存用プレイヤー(プレイヤーが前回と異なっているかの判定用)
    private PlayerController playerController;

    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();

        // 起動時のスケールをendScaleにセット
        endScale = rectTransform.localScale;

        // 初期はstartScale（0）にしておく
        rectTransform.localScale = startScale;

        oldPlayer = gameManager.selectPlayer;
        playerController = gameManager.selectPlayer.GetComponent<PlayerController>();
    }

    private void OnDisplay()
    {
        image.enabled = true;
        // 拡大アニメーション（表示）
        rectTransform.localScale = startScale;
        rectTransform.DOScale(endScale, appearDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => isRotating = true); // 拡大後に回転開始
    }

    private void OnHide()
    {
        // 回転停止と縮小アニメーション（非表示）
        isRotating = false;

        // DOTweenはOnDisable中のTweenが再生されないため、ここでは一度有効化→遅延実行が必要
        rectTransform.DOScale(startScale, disappearDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => image.enabled = false); // 最後に非表示にする
    }

    private void Update()
    {
        if (isRotating)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if(gameManager.selectPlayer != oldPlayer)
        {
            playerController = gameManager.selectPlayer.GetComponent<PlayerController>();
        }

        if(image.enabled == true && !playerController.canSkill)
        {
            OnHide();
        }
        if(image.enabled == false && playerController.canSkill)
        {
            OnDisplay();
        }

        oldPlayer = gameManager.selectPlayer;
    }
}
