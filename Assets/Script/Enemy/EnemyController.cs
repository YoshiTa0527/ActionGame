using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    /// <summary>接地判定に関するフィールド</summary>
    public static bool m_EnemyIsGround { get; set; }
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_collider;
    RaycastHit m_hit;
    private void Start()
    {
        m_collider = GetComponent<CapsuleCollider>();
    }
    private void Update()
    {
        if (IsGround())
        {
            m_EnemyIsGround = true;
            Debug.Log($"{this.gameObject.name}は接地しています");
        }
        else
        {
            m_EnemyIsGround = false;
            Debug.Log($"{this.gameObject.name}は接地していないです");
        }
    }

    private bool IsGround()
    {
        Ray ray = new Ray(this.transform.position + m_collider.center, Vector3.down);
        Debug.DrawRay(this.transform.position + m_collider.center, Vector3.down, Color.red);
        return Physics.SphereCast(ray, m_sphereRadius, out m_hit, m_rayMaxDistance, m_groundMask);
    }
    [SerializeField] bool isEnable = false;
    private void OnDrawGizmos()
    {
        if (isEnable == false)
            return;
        Gizmos.DrawWireSphere(this.transform.position + m_collider.center + Vector3.down * m_rayMaxDistance, m_sphereRadius);
    }
}
