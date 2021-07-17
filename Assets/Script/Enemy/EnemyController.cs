using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    [SerializeField] bool m_isDebugMode = true;

    int m_currentHp;
    [SerializeField] int m_enemyMaxHp = 10;
    [SerializeField] Slider m_enemyHpSlider = default;
    /// <summary>接地判定に関するフィールド</summary>
    public static bool m_EnemyIsGround { get; set; }
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_collider;
    RaycastHit m_hit;
    TargetController m_targetController;
    private void Start()
    {
        m_collider = GetComponent<CapsuleCollider>();
        m_targetController = GetComponent<TargetController>();
        m_enemyHpSlider.gameObject.SetActive(false);
        m_currentHp = m_enemyMaxHp;
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

    /// <summary>
    /// これを呼び出すと敵にダメージを与える
    /// </summary>
    /// <param name="damage"></param>
    public void Hit(int damage)
    {
        if (m_enemyHpSlider && !m_enemyHpSlider.IsActive())
        {
            m_enemyHpSlider.gameObject.SetActive(true);
        }
        m_currentHp -= damage;
        DOTween.To(() => m_enemyHpSlider.value, v =>
            {
                if (v <= 0)
                {
                    if (m_isDebugMode) m_currentHp = m_enemyMaxHp;
                    else Destroy(this.gameObject);
                }
                m_enemyHpSlider.value = v;
            }, (float)m_currentHp / m_enemyMaxHp, 1f).SetEase(Ease.OutCubic);
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
