using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵を浮かばせる
/// </summary>
public class UpperAtack : Atack
{
    [SerializeField] float m_pushPower = 10f;

    private void OnTriggerEnter(Collider other)
    {
        OnAtack(other);
    }

    protected override void OnAtack(Collider other)
    {
        Rigidbody[] rbs = other.gameObject.GetComponents<Rigidbody>();
        foreach (var item in rbs)
        {
            item.AddForce(Vector3.up * m_pushPower, ForceMode.Impulse);
        }
        base.OnAtack(other);
    }
}
