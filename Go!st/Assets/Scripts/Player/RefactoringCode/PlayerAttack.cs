using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] float attackSpeed;//�U���̑��x��
    [SerializeField] float attackCooldownTime = 1f; // �ʏ�U���̃N�[���^�C���i�b)
    [SerializeField] float attackDis = 10f;    //�I�[�g�G�C���͈́B���D�݂ŁB
    [SerializeField,Header("�e�̔��ˈʒu��Y������")] float offsetY = 0;

    [SerializeField] private BulletManager bulletManager;

    //�I�[�g�G�C���p�p�x�ϐ�
    private float degreeAttack = 0.0f;
    private float radAttack = 0.0f;
    private float nearestEnemyDis;

    private int bulletNum = 1;

    public void Attack()
    {
        nearestEnemyDis = attackDis;

        Shot(transform.forward);
    }

    void Shot(Vector3 _aimDirection)
    {
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject attackObj = bulletManager.GetBullet();
            attackObj.transform.position = this.transform.position + new Vector3(0, offsetY,0);
            attackObj.transform.rotation = Quaternion.identity;

            PlayerBulletController attackObjPlayerBullet = attackObj.GetComponent<PlayerBulletController>();
            Rigidbody attackObjRb = attackObj.GetComponent<Rigidbody>();

            attackObjPlayerBullet.Display();

            // �O�̂��߁A�O�̑��x���[���ɂ���
            attackObjRb.velocity = Vector3.zero;

            float angle = 90f * i;
            Vector3 rotatedDirection = Quaternion.AngleAxis(angle, Vector3.up) * _aimDirection;
            attackObjRb.velocity = rotatedDirection * attackSpeed;
        }
    }

    public void SetBulletNum(int _bulletNum)
    {
        bulletNum = _bulletNum;
    }

    public void SetAttackSpeed(float _attackSpeed)
    {
        attackSpeed = _attackSpeed;
    }

    public void SetAttackCooldownTime(float _attackCooldownTime)
    {
        attackCooldownTime = _attackCooldownTime;
    }

    public float GetAttackCooldownTime() => attackCooldownTime;
}
