using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//�A�j���[�V�����C�x���g�ŌĂяo�������֐��ƃA�j���[�^�[���Ⴄ�I�u�W�F�N�g�ɂ��Ă���ꍇ�̒��p�p�X�N���v�g
public class AnimationEventBridge : MonoBehaviour
{
    [SerializeField] private UnityEvent onAnimationEvent;

    public void CallEvent()
    {
        onAnimationEvent?.Invoke();
    }
}