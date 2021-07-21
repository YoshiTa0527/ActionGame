using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atack : MonoBehaviour
{
    [SerializeField] int m_atackPower = 10;
    private void OnTriggerEnter(Collider other)
    {
        OnAtack(other, AtackType.Normal);
    }

    protected virtual void OnAtack(Collider other, AtackType atackType)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"hit{other.name}");
            EnemyController[] enemyControllers = other.gameObject.GetComponents<EnemyController>();
            foreach (var item in enemyControllers)
            {
                item.Hit(m_atackPower, atackType);
            }
        }
    }


}
public enum AtackType
{
    Normal, Push, KnockOff, LiftUp
}
