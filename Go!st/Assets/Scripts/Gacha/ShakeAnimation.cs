using UnityEngine;
using DG.Tweening;

public class ShakeAnimation : MonoBehaviour
{
    [Header("揺れ設定")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float strength = 5f;
    [SerializeField] private int vibrato = 30;

    [SerializeField] private GachaPullItem gachaPullItem;

    public void StartShake()
    {
        // DOShakePositionで1回だけ揺らす
        transform.DOShakePosition(
            shakeDuration,
            new Vector3(strength, 0, 0), // 横方向のみ
            vibrato,
            90,       // randomness
            false,    // fadeOutをfalseにすると正確にdurationで終わる
            false     // snapping
        ).OnComplete(() =>
        {
            // 揺れ終了時に呼びたい処理
            gachaPullItem.GraveOver();
            Debug.Log("揺れ終了");
        });
    }
}
