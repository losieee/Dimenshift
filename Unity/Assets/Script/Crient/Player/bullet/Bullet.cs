using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AttackType
{
    Normal_Attack,
    Qkey_Attack,
    Wkey_Attack,
    Ekey_Attack,
}

public class Bullet : MonoBehaviour
{
    public AttackType attackType;
    // public NormalEnemy NormalEnemy;              // ���� ���� ���� ������Ʈ ��������
    public PlayerSO playerSO;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            switch (attackType)
            {
                case AttackType.Normal_Attack:
                    NormalAttack();

                    break;
                case AttackType.Qkey_Attack:
                    QkeyAttack();
                    break;
                case AttackType.Wkey_Attack:
                    WkeyAttack();
                    break;
                case AttackType.Ekey_Attack:
                    EkeyAttack();
                    break;
            }
        }

    }

    public void NormalAttack()
    {
        //�Ϲ� ���� ó��
        Debug.Log("�Ϲݰ��� ������");
    }

    public void QkeyAttack()
    {
        //Q ���� ó��
        Debug.Log("Q��ų ���� ������");
    }

    public void WkeyAttack()
    {
        //W ���� ó��
        Debug.Log("W��ų ���� ������");
    }

    public void EkeyAttack()
    {
        //E ���� ó��
        Debug.Log("E��ų ���� ������");
    }
}
