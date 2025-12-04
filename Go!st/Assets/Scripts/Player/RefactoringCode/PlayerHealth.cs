using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;
    [SerializeField] private PlayerHpImage playerHpImage;
    [SerializeField] private GameManager gameManager;

    private int hp;
    private RectTransform playerHpImageRect;

    private void Start()
    {
        playerHpImageRect = playerHpImage.GetComponent<RectTransform>();
    }

    public void Init()
    {
        hp = maxHP;
        UpdateHpUI();
    }

    public void Damage(int damageAmount)
    {
        hp -= damageAmount;
        UpdateHpUI();
        PlayerHPImageShake();

        if (hp <= 0)
        {
            hp = 0;
            gameManager.EndGame();
        }
    }

    public void ApplyBuff(BuffSO buff)
    {
        if (buff.buffType == BuffType.Heal)
        {
            if (hp < maxHP)
            {
                hp = Mathf.Min(hp + buff.healAmount, maxHP);
                UpdateHpUI();
                Debug.Log($"HP‰ñ•œF{buff.healAmount} ¨ Œ»İHP: {hp}");
            }
        }
    }

    private void UpdateHpUI()
    {
        playerHpImage.UpdateHp(hp);
    }

    public int GetHp() => hp;

    private void PlayerHPImageShake()
    {
        playerHpImageRect.DOShakeAnchorPos(
        duration: 0.25f,              // —h‚ê‚éŠÔ
        strength: new Vector2(20f, 20f), // —h‚ê•
        vibrato: 20,                // U“®‚Ì×‚©‚³
        randomness: 90f,            // —h‚ê‚é•ûŒü‚Ìƒ‰ƒ“ƒ_ƒ€«
        snapping: false,
        fadeOut: true
        );
    }
}