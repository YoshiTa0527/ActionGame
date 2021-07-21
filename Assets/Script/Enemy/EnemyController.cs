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
    [SerializeField] Text m_enemyStateText = default;
    [SerializeField] bool m_isDebagMode = false;
    TargetController m_targetController;
    Rigidbody m_rb;
    float m_timer;
    [SerializeField] float m_floatDuration = 3f;
    [SerializeField] GameObject m_explosionPrefab = default;


    /// <summary>接地判定に関するフィールド</summary>
    public bool EnemyIsGround { get => IsGround(); }

    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    [SerializeField] float m_rayForDistanceGround = 5f;
    public EnemyState EnemyStateProp { get; set; }
    RigidbodyConstraints m_defaultConstraints;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_collider;
    RaycastHit m_hit;

    private void Start()
    {
        m_collider = GetComponent<CapsuleCollider>();
        m_targetController = GetComponent<TargetController>();
        m_rb = GetComponent<Rigidbody>();
        m_enemyHpSlider.gameObject.SetActive(false);
        m_currentHp = m_enemyMaxHp;
        m_defaultConstraints = this.m_rb.constraints;
        EnemyStateProp = EnemyState.Idle;
    }
    private void Update()
    {
        switch (EnemyStateProp)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Damaged:
                Damaged();
                break;
            case EnemyState.LifetdUp:
                LiftedUp();
                break;
            case EnemyState.KnokedOff:
                if (IsGround())
                {
                    Debug.Log("Kno");
                    Instantiate(m_explosionPrefab, this.transform.position, Quaternion.identity).GetComponent<ExplosionForce>().AddExplosionForce();
                    Idle();
                }
                break;
            default:
                break;
        }
    }

    private void LiftedUp()
    {
        if (m_isDebugMode) m_enemyStateText.text = "EnemyState\nLiftedUp";
        EnemyStateProp = EnemyState.LifetdUp;
        /*十分地上から離れたら減速する*/
        if (!IsEnoughDistanceFromGround() && this.m_rb.velocity.y > 0)
        {
            Vector3 velo = m_rb.velocity;
            velo.y *= 0.9f;
            m_rb.velocity *= 0.95f;
        }
        /*下降し始めたら動きを止め、一定時間後に落ちる*/
        /*下降後、タイマーが動き始まる前にプレイヤーがジャンプなどで触れるとタイマーが動かなくなるので、とりあえずｘかｚに動いていたらタイマーをスタートさせることにする*/
        if (this.m_rb.velocity.y < 0)
        {
            m_rb.useGravity = false;

            m_timer += Time.deltaTime;
            Debug.Log($"EnemyTimer::{m_timer}");
            if (m_timer > m_floatDuration || IsGround())
            {
                m_timer = 0;
                m_rb.useGravity = true;

                Idle();
            }
        }
    }

    void KnokedOff()
    {
        if (m_isDebugMode) m_enemyStateText.text = "EnemyState\nKnockedOff";

        EnemyStateProp = EnemyState.KnokedOff;
    }

    private void Damaged()
    {
        if (m_isDebugMode) m_enemyStateText.text = "EnemyState\nDamaged";
        EnemyStateProp = EnemyState.Damaged;
    }

    private void Idle()
    {
        if (m_isDebugMode) m_enemyStateText.text = "EnemyState\nIdle";
        EnemyStateProp = EnemyState.Idle;
    }

    /// <summary>
    /// これを呼び出すと敵にダメージを与える
    /// </summary>
    /// <param name="damage"></param>
    public void Hit(int damage, AtackType atackType)
    {
        if (m_enemyHpSlider && !m_enemyHpSlider.IsActive())
        {
            m_enemyHpSlider.gameObject.SetActive(true);
        }

        switch (atackType)
        {
            case AtackType.Normal:
                Damaged();
                break;
            case AtackType.Push:
                break;
            case AtackType.LiftUp:
                LiftedUp();
                break;
            case AtackType.KnockOff:
                /*衝撃波を生む*/
                KnokedOff();
                break;
            default:
                break;
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
    /// <summary>
    /// 固定する
    /// </summary>
    public void EnableConstraints()
    {
        this.m_rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    /// <summary>
    /// 固定を解除する
    /// </summary>
    public void DisableConstraints()
    {
        this.m_rb.constraints = m_defaultConstraints;
    }

    private bool IsGround()
    {
        Ray ray = new Ray(this.transform.position + m_collider.center, Vector3.down);
        Debug.DrawRay(this.transform.position + m_collider.center, Vector3.down, Color.red);
        return Physics.SphereCast(ray, m_sphereRadius, out m_hit, m_rayMaxDistance, m_groundMask);
    }

    private bool IsEnoughDistanceFromGround()
    {
        Ray ray = new Ray(this.transform.position + m_collider.center, Vector3.down);

        return Physics.Raycast(ray, m_rayForDistanceGround, m_groundMask);
    }

    [SerializeField] bool m_isEnable = false;
    private void OnDrawGizmos()
    {
        if (m_isEnable == false)
            return;
        Gizmos.DrawWireSphere(this.transform.position + m_collider.center + Vector3.down * m_rayMaxDistance, m_sphereRadius);
        Gizmos.DrawLine(this.transform.position + m_collider.center, this.transform.position + m_collider.center + m_rayForDistanceGround * Vector3.down);
    }

    public enum EnemyState
    {
        Idle, Damaged, LifetdUp, KnokedOff,
    }
}
