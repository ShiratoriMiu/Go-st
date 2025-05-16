using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ResultText : MonoBehaviour
{
    void OnEnable()
    {
        Vector3 targetPosition = this.transform.localPosition;
        this.transform.localPosition += new Vector3(0, 1000, 0);
        this.transform.DOLocalMove(targetPosition, 1f).SetEase(Ease.OutBounce);
    }
}
