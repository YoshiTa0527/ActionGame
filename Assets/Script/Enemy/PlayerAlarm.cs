using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// このオブジェクトにプレイヤーが触れると、登録された敵グループにプレイヤーの存在を知らせる
/// </summary>
public class PlayerAlarm : MonoBehaviour
{
    [SerializeField] UnityEngine.Events.UnityEvent m_onTriggerPlayer;
    [SerializeField] GameObject m_enemyGroup;
    PlayerController m_player;
    bool m_isCalled;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_player = other.GetComponent<PlayerController>();
            m_onTriggerPlayer?.Invoke();
        }
    }

    public void AlarmOnEvent()
    {
        if (m_isCalled) return;
        m_isCalled = true;
        for (int i = 0; i < m_enemyGroup.transform.childCount; i++)
        {
            EnemyController enemy = m_enemyGroup.transform.GetChild(i).gameObject.GetComponent<EnemyController>();
            enemy.SetPlayer(m_player);
            enemy.EnemyStateProp = EnemyController.EnemyState.OnBattle;
            Debug.Log("名前:" + enemy.name);
        }
    }
}
