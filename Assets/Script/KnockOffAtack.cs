using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockOffAtack : Atack
{
    [SerializeField] float m_downForce = 10f;
    [SerializeField] Vector3 m_addForceDir;

    private void OnTriggerEnter(Collider other)
    {
        OnAtack(other, AtackType.KnockOff);
    }
    protected override void OnAtack(Collider other, AtackType atackType)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        other.GetComponent<EnemyController>()?.DisableConstraints();
        rb.AddForce(((other.transform.position - this.transform.position) + Vector3.down) * m_downForce, ForceMode.Impulse);
        base.OnAtack(other, atackType);
    }
}
