using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuardEvent : MonoBehaviour
{
    [SerializeField] UnityEvent m_onGuardSuccess;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AtackCollider"))
        {
            Debug.Log("HitAtackCol");
            m_onGuardSuccess?.Invoke();
        }
    }
}
