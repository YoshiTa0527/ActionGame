using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGen : MonoBehaviour
{
    [SerializeField] GameObject m_obj;
    [SerializeField] float m_interval = 1f;
    float m_timer;

    private void Update()
    {
        m_timer += Time.deltaTime;

        if (m_timer > m_interval)
        {
            m_timer = 0;
            Instantiate(m_obj, this.transform.position, Quaternion.identity);
        }
    }
}
