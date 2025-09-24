using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerSkillAnim : MonoBehaviour
{
    SpriteRenderer skillGhostRenderer;

    // 移動量（上下に1ユニット）
    [SerializeField] float moveAmount = 1f;
    [SerializeField] float duration = 1f;
    [SerializeField] float waitTime = 0.5f;

    [System.Serializable]
    public class GhostSprite
    {
        public Sprite sprite;
        public int comboNum;
    }

    [SerializeField] GhostSprite[] ghostSprites;

    [SerializeField] GameObject twinkleImage;

    private void Start()
    {
        skillGhostRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayerSkillAnimPlay(System.Action onComplete, int enemyNum)
    {
        Vector3 startPos = transform.position;

        if (float.IsNaN(startPos.x) || float.IsNaN(startPos.y) || float.IsNaN(startPos.z))
        {
            Debug.LogError("開始位置がNaNです！アニメーション中止");
            return;
        }

        // デフォルトで最後のSpriteにしておく
        skillGhostRenderer.sprite = ghostSprites[ghostSprites.Length - 1].sprite;
        twinkleImage.SetActive(true);

        //スプライト切り替え
        for (int i = 0; i < ghostSprites.Length; i++)
        {
            var ghost = ghostSprites[i];

            if (enemyNum < ghost.comboNum)
            {
                skillGhostRenderer.sprite = ghost.sprite;

                //最後の要素じゃなかったら twinkleImage を無効化
                if (i != ghostSprites.Length - 1)
                {
                    twinkleImage.SetActive(false);
                }

                break;
            }
        }


        Vector3 upPos = startPos + Vector3.up * moveAmount;

        if (float.IsNaN(upPos.x) || float.IsNaN(upPos.y) || float.IsNaN(upPos.z))
        {
            Debug.LogError("移動先がNaNです！アニメーション中止");
            return;
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveY(upPos.y, duration).SetEase(Ease.OutCubic))
           .AppendInterval(waitTime)
           .Append(transform.DOMoveY(startPos.y, duration).SetEase(Ease.OutCubic))
           .OnComplete(() =>
           {
               onComplete?.Invoke();
           });
    }

}
