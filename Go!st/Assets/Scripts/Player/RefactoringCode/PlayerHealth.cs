using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;
    [SerializeField] private PlayerHpImage playerHpImage;
    [SerializeField] private GameManager gameManager;

    private int hp;

    public void Init()
    {
        hp = maxHP;
        UpdateHpUI();
    }

    public void Damage(int damageAmount)
    {
        hp -= damageAmount;
        UpdateHpUI();

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
                Debug.Log($"HP‰ñ•œF{buff.healAmount} ¨ Œ»ÝHP: {hp}");
            }
        }
    }

    private void UpdateHpUI()
    {
        playerHpImage.UpdateHp(hp);
    }

    public int GetHp() => hp;
}