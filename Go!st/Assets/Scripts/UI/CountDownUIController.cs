using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CountDownUIController : MonoBehaviour
{
    [SerializeField] Image countDownImage;
    [SerializeField] Text countDownText;
    [SerializeField] float startCountDownDuration = 3.0f;
    [SerializeField] float finishCountDownDuration = 10.0f;
    [SerializeField] float goDisplayTime = 0.8f;
    [SerializeField] Color startCountDownColor;
    [SerializeField] Color finishCountDownColor;

    private int countDownCount;
    private float countDownElapsedTime;
    private Action<bool> onCountDownCompleteBool;
    private Action onCountDownComplete;

    public void StartCountDownStart(Action<bool> onComplete = null)
    {
        SetColor(startCountDownColor);
        onCountDownComplete = null;
        onCountDownCompleteBool = null;
        onCountDownCompleteBool = onComplete;
        StopAllCoroutines();
        StartCoroutine(StartCountDown());
    }

    public void FinishCountDownStart(Action onComplete = null)
    {
        SetColor(finishCountDownColor);
        onCountDownComplete = null;
        onCountDownCompleteBool = null;
        onCountDownComplete = onComplete;
        StopAllCoroutines();
        StartCoroutine(FinishCountDownCoroutine());
    }

    IEnumerator StartCountDown()
    {
        countDownCount = 0;
        countDownElapsedTime = 0;

        countDownImage.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(true);

        countDownText.text = Mathf.FloorToInt(startCountDownDuration).ToString();
        countDownImage.fillAmount = 0f;

        // カウントダウンループ
        while (countDownElapsedTime < startCountDownDuration)
        {
            countDownElapsedTime += Time.deltaTime;
            countDownImage.fillAmount = countDownElapsedTime % 1.0f;

            // 秒が変わった瞬間に更新
            if (countDownCount < Mathf.FloorToInt(countDownElapsedTime))
            {
                countDownCount++;

                int remaining = Mathf.FloorToInt(startCountDownDuration - countDownCount);

                if (remaining <= 0)
                {
                    // 「GO!」の表示とアニメーション
                    countDownText.text = "GO!";
                    countDownText.transform.localScale = Vector3.zero;

                    // ポンッと出るようなスケール演出（戻さないバージョン）
                    countDownText.transform
                        .DOScale(1.6f, 0.5f)
                        .SetEase(Ease.OutBack);

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
        //テキストサイズを元に戻す
        countDownText.transform.localScale = Vector3.one;

        // コールバック呼び出し
        onCountDownCompleteBool?.Invoke(true);
        onCountDownComplete?.Invoke();

        onCountDownComplete = null;
        onCountDownCompleteBool = null;
    }

    IEnumerator FinishCountDownCoroutine()
    {
        countDownCount = 0;
        countDownElapsedTime = 0;

        countDownImage.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(true);

        countDownText.text = Mathf.FloorToInt(finishCountDownDuration).ToString();
        countDownImage.fillAmount = 0f;

        // カウントダウンループ
        while (countDownElapsedTime < finishCountDownDuration)
        {
            countDownElapsedTime += Time.deltaTime;
            countDownImage.fillAmount = countDownElapsedTime % 1.0f;

            if (countDownCount < Mathf.FloorToInt(countDownElapsedTime))
            {
                countDownCount++;

                int remaining = Mathf.FloorToInt(finishCountDownDuration - countDownCount);

                if (remaining <= 0)
                {
                    // ← GO! を表示しないので、そのままループを抜ける
                    break;
                }
                else
                {
                    countDownText.text = remaining.ToString();
                    countDownText.transform.localScale = Vector3.one;
                }
            }

            yield return null;
        }

        // カウントダウン終了 → UI を隠す
        countDownImage.gameObject.SetActive(false);
        countDownText.gameObject.SetActive(false);
        countDownText.transform.localScale = Vector3.one;

        // コールバック呼び出し
        onCountDownCompleteBool?.Invoke(true);
        onCountDownComplete?.Invoke();

        // コールバックをクリア
        onCountDownComplete = null;
        onCountDownCompleteBool = null;
    }

    void SetColor(Color _color)
    {
        countDownImage.color = _color;
        countDownText.color = _color;
    }
}
