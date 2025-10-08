using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CountDownUIController : MonoBehaviour
{
    [SerializeField] Image countDownImage;
    [SerializeField] Text countDownText;
    [SerializeField] float countDownDuration = 3.0f;
    [SerializeField] float goDisplayTime = 0.8f;

    private int countDownCount;
    private float countDownElapsedTime;
    private Action<bool> onCountDownComplete;

    public void StartCountDown(Action<bool> onComplete = null)
    {
        onCountDownComplete = onComplete;
        StopAllCoroutines();
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        countDownCount = 0;
        countDownElapsedTime = 0;

        countDownImage.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(true);

        countDownText.text = Mathf.FloorToInt(countDownDuration).ToString();
        countDownImage.fillAmount = 0f;

        // カウントダウンループ
        while (countDownElapsedTime < countDownDuration)
        {
            countDownElapsedTime += Time.deltaTime;
            countDownImage.fillAmount = countDownElapsedTime % 1.0f;

            // 秒が変わった瞬間に更新
            if (countDownCount < Mathf.FloorToInt(countDownElapsedTime))
            {
                countDownCount++;

                int remaining = Mathf.FloorToInt(countDownDuration - countDownCount);

                if (remaining <= 0)
                {
                    // 「GO!」の表示とアニメーション
                    countDownText.text = "GO!";
                    countDownText.transform.localScale = Vector3.zero;

                    // ポンッと出るようなスケール演出
                    countDownText.transform
                        .DOScale(1.3f, 0.4f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            // 少し縮むように戻す（自然な感じに）
                            countDownText.transform
                                .DOScale(1f, 0.2f)
                                .SetEase(Ease.OutQuad);
                        });

                    break; // ループを抜ける
                }
                else
                {
                    countDownText.text = remaining.ToString();
                    countDownText.transform.localScale = Vector3.one;
                }
            }

            yield return null;
        }

        // サークル画像を非表示にする
        countDownImage.gameObject.SetActive(false);

        // 「GO!」を一定時間表示
        yield return new WaitForSeconds(goDisplayTime);

        // テキストを非表示にする
        countDownText.gameObject.SetActive(false);

        // コールバック呼び出し
        onCountDownComplete?.Invoke(true);
    }
}
