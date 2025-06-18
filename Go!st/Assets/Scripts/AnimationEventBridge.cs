using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//アニメーションイベントで呼び出したい関数とアニメーターが違うオブジェクトについている場合の中継用スクリプト
public class AnimationEventBridge : MonoBehaviour
{
    [SerializeField] private UnityEvent onAnimationEvent;

    public void CallEvent()
    {
        onAnimationEvent?.Invoke();
    }
}