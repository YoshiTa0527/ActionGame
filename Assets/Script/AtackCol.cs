using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtackCol : MonoBehaviour
{
    [SerializeField] bool m_testMode = true;

    float m_timer;

    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer > 0.1f)
        {
            m_timer = 0;
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitCollider"))
        {
            Debug.Log("HitCollider");
            other.transform.GetComponentInParent<PlayerStatus>().TakeDamage(10);
        }
        else if (other.CompareTag("GuardCollider"))
        {
            Debug.Log("Guard");
        }
    }
}
