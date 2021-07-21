using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    [SerializeField] float m_radius;
    [SerializeField] float m_explosionForce;
    private void Start()
    {
       
    }

    public void AddExplosionForce()
    {
        Collider[] cols = Physics.OverlapSphere(this.transform.position, m_radius);

        foreach (var item in cols)
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce(m_explosionForce, this.transform.position, m_radius);
                Debug.Log("addExplosiom");
            }
        }
    }
}
