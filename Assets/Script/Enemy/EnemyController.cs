using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    [SerializeField] bool m_isDebugMode = true;
    [SerializeField] UnityEngine.Events.UnityEvent m_onKnockedOff;
    [SerializeField] UnityEngine.Events.UnityEvent m_onAtack;
    public bool IsHookable { get; set; }
    [SerializeField] int m_guardGuage;
    int m_currentHp;
    [SerializeField] int m_enemyMaxHp = 10;
    [SerializeField] int m_enemyMaxGuardGuage = 10;
    [SerializeField] Slider m_enemyHpSlider = default;
    [SerializeField] Text m_enemyStateText = default;
    [SerializeField] bool m_isDebagMode = false;
    [SerializeField] float m_idleDuration = 1f;
    TargetController m_targetController;
    [SerializeField] LockOnController m_lock;
    Rigidbody m_rb;
    float m_timer;
    [SerializeField] float m_floatDuration = 3f;
    [SerializeField] float m_hookedDuration = 2f;

    [SerializeField] GameObject m_explosionPrefab = default;
    Renderer m_renderer;
    [SerializeField] float m_dirMin = 10;
    [SerializeField] float m_dirMax = 10;
    [SerializeField] float m_runSpeed = 100;
    [SerializeField] float m_walkSpeed = 100;
    [SerializeField] float m_turnSpeed = 20;
    float m_speed;
    PlayerController m_player;
    Animator m_anim;
    public void SetPlayer(PlayerController player) { m_player = player; }
    [SerializeField] float m_keepDistance = 5f;
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
        m_renderer = GetComponent<Renderer>();
        m_collider = GetComponent<CapsuleCollider>();
        m_targetController = GetComponent<TargetController>();
        m_rb = GetComponent<Rigidbody>();
        m_enemyHpSlider.gameObject.SetActive(false);
        m_currentHp = m_enemyMaxHp;
        m_defaultConstraints = this.m_rb.constraints;
        m_anim = GetComponent<Animator>();
        EnemyStateProp = EnemyState.Patrol;
    }
    private void Update()
    {
        Debug.Log("EnemyState" + EnemyStateProp.ToString());
        switch (EnemyStateProp)
        {
            case EnemyState.Patrol:
                //float dirX = Random.Range(m_dirMin, m_dirMax);
                //float dirZ = Random.Range(m_dirMin, m_dirMax);
                //Vector3 dir = new Vector3(dirX, this.transform.position.y, dirZ) - this.transform.position;
                //m_rb.velocity = dir * m_speed * Time.deltaTime;
                break;
            case EnemyState.OnBattle:
                OnBattle();
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
                    m_rb.useGravity = true;
                    Instantiate(m_explosionPrefab, this.transform.position, Quaternion.identity).GetComponent<ExplosionForce>().AddExplosionForce();
                    m_onKnockedOff?.Invoke();
                    OnBattle();
                }
                break;
            case EnemyState.Hooked:
                m_timer += Time.deltaTime;
                if (m_timer >= m_hookedDuration)
                {
                    m_timer = 0;
                    OnBattle();
                }
                break;
            case EnemyState.Escape:
                break;
            case EnemyState.Death:
                if (!m_rb.useGravity) m_rb.useGravity = true;
                DisableConstraints();
                break;
            default:
                break;
        }
    }

    private void LiftedUp()
    {
        if (m_enemyStateText) m_enemyStateText.text = "EnemyState\nLiftedUp";
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
                OnBattle();
            }
        }
    }

    void KnokedOff()
    {
        if (m_enemyStateText) m_enemyStateText.text = "EnemyState\nKnockedOff";

        EnemyStateProp = EnemyState.KnokedOff;
    }

    private void Damaged()
    {
        if (m_enemyStateText) m_enemyStateText.text = "EnemyState\nDamaged";
        m_actionTimer = 0;
        m_timer += Time.deltaTime;
        Debug.Log($"EnemyTimer::{m_timer}");
        if (m_timer > m_idleDuration)
        {
            m_timer = 0;
            OnBattle();
        }

    }
    [SerializeField] float m_actionInterval = 1;
    float m_actionTimer;
    bool m_isAbleToTurn = true;
    /// <summary>
    ///  一定距離になるまで歩きながらプレイヤーのところへ前進する
    /// </summary>
    private void OnBattle()
    {


        if (m_enemyStateText) m_enemyStateText.text = "EnemyState\nOnBattle";

        if (m_player)
        {
            Vector3 dir = default;

            if (m_currentHp <= m_enemyMaxHp / 3 && m_currentHp > 0)
            {
                Debug.Log("EnemyState:Escape");
                dir = this.transform.position - m_player.transform.position;
                dir.y = 0;
                LookAtTarget(dir);
                m_speed = m_runSpeed * 1.5f;
            }
            else
            {
                Debug.Log("EnemyState:OnBattle");
                float distance = Vector3.Distance(this.transform.position, m_player.transform.position);
                dir = m_player.transform.position - this.transform.position;
                dir.y = 0;


                LookAtTarget(dir);
                if (IsGround())
                {
                    if (distance >= m_keepDistance)
                    {
                        m_speed = m_runSpeed;

                    }
                    else if (distance <= m_keepDistance)
                    {
                        m_speed = 0;
                        m_actionTimer += Time.deltaTime;
                        if (m_actionTimer >= m_actionInterval)
                        {
                            m_actionTimer = 0;
                            DecideEnemyAction();
                        }
                    }
                }
            }
            Vector3 velo = dir.normalized * m_speed * Time.deltaTime;
            velo.y = m_rb.velocity.y;
            m_rb.velocity = velo;
        }
        EnemyStateProp = EnemyState.OnBattle;
    }

    void DecideEnemyAction()
    {
        if (Probability(50))
        {
            Atack();
        }
        else
        {
            Move();
        }
    }

    void Atack()
    {
        Debug.Log("EnemyAction:攻撃");
        m_anim.SetTrigger("Atack");
    }

    void Move()
    {
        Debug.Log("EnemyAction:移動");
        m_isAbleToTurn = true;
    }

    void LookAtTarget(Vector3 target)
    {
        if (m_isAbleToTurn)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, m_turnSpeed * Time.deltaTime);
        }
    }

    void CanTurnOnAnimation() => m_isAbleToTurn = true;

    void CanNotTurnOnAnimation() => m_isAbleToTurn = false;

    void InvokeAtackEventOnAnimation() => m_onAtack?.Invoke();

    [SerializeField] Collider m_atackCol;
    public void EnableAtackCol() => m_atackCol.gameObject.SetActive(true);
    public void DisAbleAtackCol() => m_atackCol.gameObject.SetActive(false);



    /// <summary>
    /// これを呼び出すと敵にダメージを与える
    /// </summary>
    /// <param name="damage"></param>
    public void Hit(int damage, AtackType atackType)
    {
        if (m_enemyHpSlider && !m_enemyHpSlider.gameObject.activeSelf)
        {
            m_enemyHpSlider.gameObject.SetActive(true);
        }

        switch (atackType)
        {
            case AtackType.Normal:
                EnemyStateProp = EnemyState.Damaged;
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
        m_guardGuage -= damage * 2;

        if (m_guardGuage <= 0)
        {
            // Breaked();
        }

        DOTween.To(() => m_enemyHpSlider.value, v =>
            {
                if (v <= 0)
                {
                    EnemyStateProp = EnemyState.Death;
                    if (m_isDebugMode)
                    {
                        m_currentHp = m_enemyMaxHp;
                    }
                    else Destroy(this.gameObject);
                }
                m_enemyHpSlider.value = v;
            }, (float)m_currentHp / m_enemyMaxHp, 1f).SetEase(Ease.OutCubic);
    }

    private void Breaked()
    {
        m_renderer.material.color = Color.yellow;
        EnemyStateProp = EnemyState.GuardBreaked;
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

    /// <summary>
    /// 確率を求める
    /// </summary>
    /// <param name="fPercent"></param>
    /// <returns></returns>
    public static bool Probability(float fPercent)
    {
        float fProbabilityRate = UnityEngine.Random.value * 100.0f;

        if (fPercent == 100.0f && fProbabilityRate == fPercent)
        {
            return true;
        }
        else if (fProbabilityRate < fPercent)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [SerializeField] bool m_isEnable = false;
    private void OnDrawGizmos()
    {
        if (m_isEnable == false)
            return;
        Gizmos.DrawWireSphere(this.transform.position + m_collider.center + Vector3.down * m_rayMaxDistance, m_sphereRadius);
        Gizmos.DrawLine(this.transform.position + m_collider.center, this.transform.position + m_collider.center + m_rayForDistanceGround * Vector3.down);
    }

    private void OnDestroy()
    {
        m_lock.UnLockEnemy();
    }

    public enum EnemyState
    {
        OnBattle, Damaged, LifetdUp, KnokedOff, Move, Atack, Escape, Found, GuardBreaked, Patrol, Hooked, Death
    }
}
