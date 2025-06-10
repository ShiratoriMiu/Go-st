using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ItemNumTMP : MonoBehaviour
{
    TMP_Text itemNumTMP;

    private int numEquippedItems = 0;//���ݑ������Ă���A�C�e���̐�

    [SerializeField] private float stretchDuration = 0.2f;

    private void Start()
    {
        itemNumTMP = GetComponent<TMP_Text>();
    }

    public void UpdateItemNumTMP(int _value, int _maxItemNum)
    {
        numEquippedItems += _value;

        if (numEquippedItems < 0) numEquippedItems = 0;
        else if (numEquippedItems > _maxItemNum) numEquippedItems = _maxItemNum;

        itemNumTMP.text = numEquippedItems + "/" + _maxItemNum;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();

        // 1. �y���ׂ��
        seq.Append(transform.DOScale(new Vector3(0.85f, 1.2f, 1f), stretchDuration)
            .SetEase(Ease.OutQuad));

        // 2. �t�ɂ��L�т�i�����j
        seq.Append(transform.DOScale(new Vector3(1.05f, 0.95f, 1f), stretchDuration * 0.8f)
            .SetEase(Ease.OutQuad));

        // 3. �₳�������ɖ߂�i���炩���e�ށj
        seq.Append(transform.DOScale(Vector3.one, stretchDuration * 1.2f)
            .SetEase(Ease.OutElastic)
            .SetEase(Ease.OutElastic, 0.8f, 0.25f)); // ���炩���v����

        seq.Play();


    }

}
