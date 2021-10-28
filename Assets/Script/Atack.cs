using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atack : MonoBehaviour
{
    [SerializeField] UnityEngine.Events.UnityEvent m_OnAtackSuccces;
    [SerializeField] int m_atackPower = 10;
    [SerializeField] bool m_isEnableHitStop = false;
    [SerializeField] float m_hitStopScale = 0.8f;
    [SerializeField] float m_hitStopTime = 0.5f;


    protected virtual void Update()
    {

    }

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
            if (m_isEnableHitStop)
            {
                TimeScaleManager.m_hitStopDuration = m_hitStopTime;
                TimeScaleManager.HitStop(m_hitStopScale);
            }
            m_OnAtackSuccces?.Invoke();
        }
    }


}
public enum AtackType
{
    Normal, Push, KnockOff, LiftUp
}
