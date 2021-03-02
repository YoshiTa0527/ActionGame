using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 範囲内で、最も近い敵にロックオンカーソルをつける
/// </summary>
public class DetectiveEnemy : MonoBehaviour
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

            /*フィールド上に居る敵全てを配列に入れる*/
            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");

            m_target = enemyArray.OrderBy(enemy => Vector3.Distance(this.transform.position, enemy.transform.position)).FirstOrDefault();

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
