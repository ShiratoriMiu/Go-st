using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelUpText : MonoBehaviour
{
    [SerializeField] Transform target;         // Text�Ȃǂ̎q�I�u�W�F�N�g
    [SerializeField] CanvasGroup canvasGroup;  // �t�F�[�h�p

    [SerializeField] float moveDistance = 1f;  // ��ɓ��������i���[�J��Y�����j
    [SerializeField] float fadeInDuration = 0.5f;
    [SerializeField] float stayDuration = 1.0f;
    [SerializeField] float fadeOutDuration = 0.5f;

    private Camera mainCamera;
    private Vector3 localDefaultPos;
    private Sequence animationSequence;

    void Start()
    {
        mainCamera = Camera.main;

        // �q�I�u�W�F�N�g�̃��[�J�������ʒu���L�^
        localDefaultPos = target.localPosition;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // �J�����̕����������i���[���h��Ԃ� forward �ɍ��킹��j
            target.forward = mainCamera.transform.forward;
        }
    }

    public void PlayAnimation()
    {
        // ���łɍĐ����Ȃ�~�߂�
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        target.gameObject.SetActive(true);
        canvasGroup.alpha = 0;

        // �J�n�ʒu�F�e�̃��[�J����Ԃŉ������ɃI�t�Z�b�g
        Vector3 startLocalPos = localDefaultPos - new Vector3(0, moveDistance, 0);
        target.localPosition = startLocalPos;

        // �A�j���[�V����
        animationSequence = DOTween.Sequence();
        animationSequence.Append(canvasGroup.DOFade(1, fadeInDuration));
        animationSequence.Join(target.DOLocalMove(localDefaultPos, fadeInDuration).SetEase(Ease.OutCubic));
        animationSequence.AppendInterval(stayDuration);
        animationSequence.Append(canvasGroup.DOFade(0, fadeOutDuration));
        animationSequence.OnComplete(() =>
        {
            target.gameObject.SetActive(false);
        });
    }
}
