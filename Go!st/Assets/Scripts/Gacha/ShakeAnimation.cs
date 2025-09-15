using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShakeAnimation : MonoBehaviour
{
    [Header("�h��ݒ�")]
    //�h���ŒZ����
    [SerializeField] private float minShakeDuration = 0.3f;
    //�h���Œ�����
    [SerializeField] private float maxShakeDuration = 1.0f;
    //�h��̕�
    [SerializeField] private float strength = 5f;
    //�h��ׂ̍���
    [SerializeField] private int vibrato = 30;              

    [Header("�h��Ԋu")]
    //�ŒZ
    [SerializeField] private float minInterval = 1.0f;
    //�Œ�
    [SerializeField] private float maxInterval = 3.0f;

    private void Start()
    {
        StartCoroutine(ShakeLoop());
    }

    private System.Collections.IEnumerator ShakeLoop()
    {
        while (true)
        {
            // �����_���ȗh�ꎞ�Ԃ�����
            float shakeDuration = Random.Range(minShakeDuration, maxShakeDuration);

            // �h����s�i�������̂݁j
            transform.DOShakePosition(
                shakeDuration,
                new Vector3(strength, 0, 0),
                vibrato,
                90,
                false,
                true
            );

            // �h�ꂪ�I���܂őҋ@
            yield return new WaitForSeconds(shakeDuration);

            // �����_���ȃC���^�[�o����ҋ@
            float interval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(interval);
        }
    }
}