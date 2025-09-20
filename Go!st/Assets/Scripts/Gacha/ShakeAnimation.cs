using UnityEngine;
using DG.Tweening;

public class ShakeAnimation : MonoBehaviour
{
    [Header("�h��ݒ�")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float strength = 5f;
    [SerializeField] private int vibrato = 30;

    [SerializeField] private GachaPullItem gachaPullItem;

    public void StartShake()
    {
        // DOShakePosition��1�񂾂��h�炷
        transform.DOShakePosition(
            shakeDuration,
            new Vector3(strength, 0, 0), // �������̂�
            vibrato,
            90,       // randomness
            false,    // fadeOut��false�ɂ���Ɛ��m��duration�ŏI���
            false     // snapping
        ).OnComplete(() =>
        {
            // �h��I�����ɌĂт�������
            gachaPullItem.GraveOver();
            Debug.Log("�h��I��");
        });
    }
}
