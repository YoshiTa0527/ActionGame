using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundTrees : MonoBehaviour
{
    [SerializeField] GameObject m_prefabs;
    [SerializeField] int m_count;
    [SerializeField] float m_radius;
    [SerializeField] float m_yOffset;

    private void Start()
    {
        DuplicateObjcts();
    }

    void DuplicateObjcts()
    {
        float angleDiff = 360f / (float)m_count;

        for (int i = 0; i < m_count; i++)
        {
            var go = Instantiate(m_prefabs, this.transform);
            var pos = go.transform.position;
            float angle = (90 - angleDiff * i) * Mathf.Deg2Rad;
            pos.x += m_radius * Mathf.Cos(angle);
            pos.z += m_radius * Mathf.Sin(angle);
            pos.y = m_yOffset;
            go.transform.position = pos;
        }
    }
}
