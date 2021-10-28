using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockOffAtack : Atack
{
    [SerializeField] float m_downForce = 10f;


    private void OnTriggerEnter(Collider other)
    {
        OnAtack(other, AtackType.KnockOff);
    }
    protected override void OnAtack(Collider other, AtackType atackType)
    {
        Rigidbody[] rbs = other.gameObject.GetComponents<Rigidbody>();
        foreach (var rb in rbs)
        {
            other.GetComponent<EnemyController>()?.DisableConstraints();
            rb.AddForce((this.transform.forward + Vector3.down) * m_downForce, ForceMode.Impulse);
        }


        base.OnAtack(other, atackType);
    }
}
