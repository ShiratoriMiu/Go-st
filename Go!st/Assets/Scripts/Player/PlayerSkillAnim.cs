using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerSkillAnim : MonoBehaviour
{
    // 移動量（上下に1ユニット）
    [SerializeField] float moveAmount = 1f;
    [SerializeField] float duration = 1f;
    [SerializeField] float waitTime = 0.5f;

    public void PlayerSkillAnimPlay(System.Action onComplete)
    {
        Vector3 startPos = transform.position;

        if (float.IsNaN(startPos.x) || float.IsNaN(startPos.y) || float.IsNaN(startPos.z))
        {
            Debug.LogError("開始位置がNaNです！アニメーション中止");
            return;
        }

        Vector3 upPos = startPos + Vector3.up * moveAmount;

        if (float.IsNaN(upPos.x) || float.IsNaN(upPos.y) || float.IsNaN(upPos.z))
        {
            Debug.LogError("移動先がNaNです！アニメーション中止");
            return;
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveY(upPos.y, duration).SetEase(Ease.OutCubic))
           .AppendInterval(waitTime)
           .Append(transform.DOMoveY(startPos.y, duration).SetEase(Ease.OutCubic))
           .OnComplete(() =>
           {
               onComplete?.Invoke();
           });
    }

}
