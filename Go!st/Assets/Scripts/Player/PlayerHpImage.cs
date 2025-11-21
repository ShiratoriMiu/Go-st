using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHpImage : MonoBehaviour
{
    [SerializeField] GameObject[] hpImages;

    public void UpdateHp(int _hp)
    {
        for(int i = 0; i < hpImages.Length; ++i)
        {
            if(i < _hp)
            {
                //hpImages[i].SetActive(true);
                if(!hpImages[i].activeSelf) ShowHPImage(i);
            }
            else
            {
                //hpImages[i].SetActive(false);
                if (hpImages[i].activeSelf) HideHPImage(i);
            }
        }
    }

    private void HideHPImage(int _num)
    {
        hpImages[_num].transform.DOKill(true);

        hpImages[_num].transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.Append(hpImages[_num].transform.DOScale(1.3f, 0.2f));
        seq.Append(hpImages[_num].transform.DOScale(0f, 0.3f));
        seq.OnComplete(() =>
        {
            hpImages[_num].SetActive(false);
        });
    }

    private void ShowHPImage(int _num)
    {
        hpImages[_num].transform.DOKill(true);

        GameObject heart = hpImages[_num];
        RectTransform heartRT = heart.GetComponent<RectTransform>();
        RectTransform hpPanelRT = heartRT.parent as RectTransform;
        Canvas canvas = hpPanelRT.GetComponentInParent<Canvas>();

        // 出発点 = 画面中央
        Vector2 startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hpPanelRT,
            new Vector2(Screen.width / 2f, Screen.height / 2f),
            null,
            out startPos
        );
        startPos += new Vector2(0f, 100f);

        // 到着点 = HP位置
        Vector2 targetPos = heartRT.anchoredPosition;

        // 初期化
        heart.SetActive(true);
        heartRT.anchoredPosition = startPos;
        heartRT.localScale = Vector3.zero;
        startPos += new Vector2(0f, 50f);
        // --- 制御点（円弧の高さを大きくする） ---
        // 出発点と到着点の中間よりさらに上に持ち上げる
        Vector2 controlPos = (startPos + targetPos) / 2f + new Vector2(500f, 0); // 高さ200で大げさな弧

        // DOTweenシーケンス
        Sequence seq = DOTween.Sequence();

        // スケール演出（ポンッ）
        seq.Append(heartRT.DOScale(1.8f, 0.3f));
        seq.Join(heartRT.DOMove(heartRT.transform.position + new Vector3(0f, 50f, 0f), 0.3f));
        seq.Append(heartRT.DOScale(1f, 0.1f));

        // 移動（ベジェ曲線）
        seq.Join(DOTween.To(
            () => 0f,
            t =>
            {
                Vector2 pos =
                    (1 - t) * (1 - t) * startPos +
                    2 * (1 - t) * t * controlPos +
                    t * t * targetPos;

                heartRT.anchoredPosition = pos;
            },
            1f,
            0.5f // 移動時間を少し長めにすると弧がわかりやすい
        ));
    }
}
