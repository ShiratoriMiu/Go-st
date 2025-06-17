using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillChargeEffect : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 100f;
    private bool isRotating = false;

    private RectTransform rectTransform;

    // �A�j���[�V�����ݒ�
    [SerializeField] float appearDuration = 0.3f;
    [SerializeField] float disappearDuration = 0.2f;

    private Vector3 startScale = Vector3.zero;
    private Vector3 endScale;

    [SerializeField] GameManager gameManager;

    private GameObject oldPlayer;//�ۑ��p�v���C���[(�v���C���[���O��ƈقȂ��Ă��邩�̔���p)
    private PlayerController playerController;

    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        image = GetComponent<Image>();

        // �N�����̃X�P�[����endScale�ɃZ�b�g
        endScale = rectTransform.localScale;

        // ������startScale�i0�j�ɂ��Ă���
        rectTransform.localScale = startScale;

        oldPlayer = gameManager.selectPlayer;
        playerController = gameManager.selectPlayer.GetComponent<PlayerController>();
    }

    private void OnDisplay()
    {
        image.enabled = true;
        // �g��A�j���[�V�����i�\���j
        rectTransform.localScale = startScale;
        rectTransform.DOScale(endScale, appearDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => isRotating = true); // �g���ɉ�]�J�n
    }

    private void OnHide()
    {
        // ��]��~�Ək���A�j���[�V�����i��\���j
        isRotating = false;

        // DOTween��OnDisable����Tween���Đ�����Ȃ����߁A�����ł͈�x�L�������x�����s���K�v
        rectTransform.DOScale(startScale, disappearDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => image.enabled = false); // �Ō�ɔ�\���ɂ���
    }

    private void Update()
    {
        if (isRotating)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if(gameManager.selectPlayer != oldPlayer)
        {
            playerController = gameManager.selectPlayer.GetComponent<PlayerController>();
        }

        if(image.enabled == true && !playerController.canSkill)
        {
            OnHide();
        }
        if(image.enabled == false && playerController.canSkill)
        {
            OnDisplay();
        }

        oldPlayer = gameManager.selectPlayer;
    }
}
