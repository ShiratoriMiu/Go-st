using UnityEngine;
using UnityEngine.Events;

public class AnimationEventReceiver : MonoBehaviour
{
    // インスペクターから関数を設定可能
    public UnityEvent OnEvent;

    // アニメーションイベントから呼ばれる関数
    public void OnCustomEvent()
    {
        OnEvent?.Invoke();
    }
}