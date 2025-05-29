using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerSkillAnim : MonoBehaviour
{
    // �ړ��ʁi�㉺��1���j�b�g�j
    [SerializeField] float moveAmount = 1f;
    [SerializeField] float duration = 1f;
    [SerializeField] float waitTime = 0.5f;

    public void PlayerSkillAnimPlay(System.Action onComplete)
    {
        Vector3 startPos = transform.position;

        if (float.IsNaN(startPos.x) || float.IsNaN(startPos.y) || float.IsNaN(startPos.z))
        {
            Debug.LogError("�J�n�ʒu��NaN�ł��I�A�j���[�V�������~");
            return;
        }

        Vector3 upPos = startPos + Vector3.up * moveAmount;

        if (float.IsNaN(upPos.x) || float.IsNaN(upPos.y) || float.IsNaN(upPos.z))
        {
            Debug.LogError("�ړ��悪NaN�ł��I�A�j���[�V�������~");
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
