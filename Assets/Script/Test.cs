using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Test : MonoBehaviour
{
    [SerializeField] bool m_a;
    [SerializeField] GameObject m_b;
    [SerializeField] float m_rayMaxDis = 1f;
    [SerializeField] LayerMask m_mask;
    private void Update()
    {
        if (m_a)
        {
            RaycastHit m_hit;
            Debug.DrawRay(this.transform.position, Vector3.down * m_rayMaxDis, Color.blue);
            if (Physics.Raycast(this.transform.position, Vector3.down, out m_hit, m_rayMaxDis, m_mask)) Debug.Log("test::true::" + m_hit.collider.name);
            else { Debug.Log("test::false"); }
        }

    }

    bool CalcValue(float x, float threshold)
    {
        if (-1 * threshold < x && x < threshold) return true;
        else return false;
    }
}
