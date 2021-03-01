using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 範囲内で、最も近い敵をロックオンする   
/// </summary>
public class LockOnController : MonoBehaviour
{
    [SerializeField] float m_searchRange = 4f;
    [SerializeField] float m_lockonInterval = 1f;
    GameObject m_target = null;
    float m_timer;

    public GameObject GetTarget
    {
        get { return m_target; }
    }

    private void Update()
    {
        m_timer += Time.deltaTime;

        if (m_timer > m_lockonInterval)
        {
            m_timer = 0;

            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (var enemy in enemyArray)
            {
                float dis = Vector3.Distance(this.transform.position, enemy.transform.position);

                if (dis < m_searchRange)
                {
                    if (m_target == null || dis < Vector3.Distance(this.transform.position, m_target.transform.position))
                    {
                        m_target = enemy;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ターゲット送り。現在ターゲットしている敵の次の敵をロックオンする
    /// </summary>
    public GameObject GetNextTarget
    {
        get { return m_target; }
    }
}
