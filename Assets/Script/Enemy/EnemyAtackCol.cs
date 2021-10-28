using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtackCol : MonoBehaviour
{
    [SerializeField] UnityEngine.Events.UnityEvent m_onAtackSuccess;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("HitCollider"))
        {
            m_onAtackSuccess?.Invoke();
        }
    }
}
