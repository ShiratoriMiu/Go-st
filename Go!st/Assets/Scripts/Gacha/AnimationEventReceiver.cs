using UnityEngine;
using UnityEngine.Events;

public class AnimationEventReceiver : MonoBehaviour
{
    // �C���X�y�N�^�[����֐���ݒ�\
    public UnityEvent OnEvent;

    // �A�j���[�V�����C�x���g����Ă΂��֐�
    public void OnCustomEvent()
    {
        OnEvent?.Invoke();
    }
}