using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        OnAtack(other);
    }

    protected virtual void OnAtack(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"hit{other.name}");
            EnemyController[] enemyControllers = other.gameObject.GetComponents<EnemyController>();
            foreach (var item in enemyControllers)
            {
                item.Hit(10);

            }
        }
    }
}
