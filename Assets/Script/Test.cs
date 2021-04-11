using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Test : MonoBehaviour
{
    [SerializeField] GameObject m_a;
    [SerializeField] GameObject m_b;
    private void Update()
    {
        if (Mathf.Approximately(m_a.transform.position.y, m_b.transform.position.y))
        {
            Debug.Log($"aの高さ：{m_a.transform.position.y}::ｂの高さ:{m_b.transform.position.y}");
        }
        float disAbs = Mathf.Abs(m_a.transform.position.y - m_b.transform.position.y);
        if (CalcValue(disAbs, 0.5f))
            Debug.Log($"範囲内");
        else Debug.Log("範囲外");
    }

    bool CalcValue(float x, float threshold)
    {
        if (-1 * threshold < x && x < threshold) return true;
        else return false;
    }
}
