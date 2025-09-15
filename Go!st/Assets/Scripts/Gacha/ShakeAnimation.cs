using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShakeAnimation : MonoBehaviour
{
    [Header("—h‚êİ’è")]
    //—h‚ê‚éÅ’ZŠÔ
    [SerializeField] private float minShakeDuration = 0.3f;
    //—h‚ê‚éÅ’·ŠÔ
    [SerializeField] private float maxShakeDuration = 1.0f;
    //—h‚ê‚Ì•
    [SerializeField] private float strength = 5f;
    //—h‚ê‚Ì×‚©‚³
    [SerializeField] private int vibrato = 30;              

    [Header("—h‚êŠÔŠu")]
    //Å’Z
    [SerializeField] private float minInterval = 1.0f;
    //Å’·
    [SerializeField] private float maxInterval = 3.0f;

    private void Start()
    {
        StartCoroutine(ShakeLoop());
    }

    private System.Collections.IEnumerator ShakeLoop()
    {
        while (true)
        {
            // ƒ‰ƒ“ƒ_ƒ€‚È—h‚êŠÔ‚ğŒˆ’è
            float shakeDuration = Random.Range(minShakeDuration, maxShakeDuration);

            // —h‚êÀsi‰¡•ûŒü‚Ì‚İj
            transform.DOShakePosition(
                shakeDuration,
                new Vector3(strength, 0, 0),
                vibrato,
                90,
                false,
                true
            );

            // —h‚ê‚ªI‚í‚é‚Ü‚Å‘Ò‹@
            yield return new WaitForSeconds(shakeDuration);

            // ƒ‰ƒ“ƒ_ƒ€‚ÈƒCƒ“ƒ^[ƒoƒ‹‚ğ‘Ò‹@
            float interval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(interval);
        }
    }
}