using UnityEngine;
using DG.Tweening;

public class ShakeAnimation : MonoBehaviour
{
    [Header("—h‚êİ’è")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float strength = 5f;
    [SerializeField] private int vibrato = 30;

    [SerializeField] private GachaPullItem gachaPullItem;

    public void StartShake()
    {
        // DOShakePosition‚Å1‰ñ‚¾‚¯—h‚ç‚·
        transform.DOShakePosition(
            shakeDuration,
            new Vector3(strength, 0, 0), // ‰¡•ûŒü‚Ì‚İ
            vibrato,
            90,       // randomness
            false,    // fadeOut‚ğfalse‚É‚·‚é‚Æ³Šm‚Éduration‚ÅI‚í‚é
            false     // snapping
        ).OnComplete(() =>
        {
            // —h‚êI—¹‚ÉŒÄ‚Ñ‚½‚¢ˆ—
            if(GachaController.Instance.pullNum == 9)gachaPullItem.GraveOver();
            Debug.Log("—h‚êI—¹");
        });
    }
}
