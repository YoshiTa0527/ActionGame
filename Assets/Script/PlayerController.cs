using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの動きを決める。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    /// <summary>プレイヤーの動きに関するフィールド</summary>
    float m_horizontal;
    float m_virtical;
    Vector3 m_dir;
    [SerializeField] AudioManager m_audioManager;
    [SerializeField] float m_defaultMovingSpeed = 5f;
    [SerializeField] float m_sprintSpeed = 8f;
    [SerializeField] float m_lockOnMoveSpeed = 3f;
    [SerializeField] float m_dodgeSpeed = 8f;
    [SerializeField] float m_turnSpeed = 5f;
    [SerializeField] float m_jumpPower = 5f;
    [SerializeField] float m_limitSpeed = 20f;
    [SerializeField] float m_decelerateSpeed = 1.1f;
    [SerializeField] float m_landingMotionDis = 3f;
    [SerializeField] float m_fallMotionDis = 1f;
    [SerializeField] float m_movingPowerInTheAir = 2f;
    float m_spdTemp;
    float m_turnSpdTemp;
    [SerializeField] bool m_canMove = true;
    bool m_isDodging = false;
    int m_jumpAtackCounter;
    [SerializeField] Text m_debugSpdText;
    float m_maxSpeed;
    static public bool IsSprint { get; set; }
    /// <summary>接地判定に関するフィールド</summary>
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    public bool GetIsGrounded { get => IsGround(); }
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_colider;
    /// <summary>ロックオンに関するフィールド</summary>
    LockOnController m_loc;
    /// <summary>攻撃判定に使うコライダー</summary>
    [SerializeField] Collider m_atackCol;
    [SerializeField] Collider m_upperAtackCol;
    [SerializeField] Collider m_upperKnockOffCol;
    /// <summary>被弾判定に用いるコライダー</summary>
    [SerializeField] Collider m_hitCol;
    /// <summary>ガードに用いるコライダー</summary>
    [SerializeField] Collider m_guardCol;

    /// <summary>回避するときの消費魔力</summary>
    [SerializeField] int m_manaCostDodge = 5;
    /// <summary>グラップルするときの消費魔力</summary>
    [SerializeField] int m_manaCostGrapple = 5;

    Rigidbody m_rb;
    Animator m_anim;

    NewGrapplingManager m_grapple;
    [SerializeField] PlayerStatus m_status;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_loc = FindObjectOfType<LockOnController>();
        m_colider = GetComponent<CapsuleCollider>();
        m_spdTemp = m_defaultMovingSpeed;
        m_turnSpdTemp = m_turnSpeed;
        m_grapple = GetComponent<NewGrapplingManager>();

        m_anim = GetComponent<Animator>();

    }


    private void Update()
    {
        Debug.Log("velocity.y=" + m_rb.velocity.y);

        if (IsGround()) m_jumpAtackCounter = 0;
        Debug.Log($"m_jumpAtackCounter{ m_jumpAtackCounter}");

        #region 入力を受け取る処理
        switch (PlayerState.m_PlayerStates)
        {
            case PlayerStates.InGame:
                if (Input.GetButtonDown("Atack"))
                {
                    if (IsGround())
                    {
                        m_anim.SetTrigger("Atack");
                    }
                    else
                    {
                        m_jumpAtackCounter++;
                        if (m_jumpAtackCounter <= 1)
                        {
                            m_anim.SetTrigger("JumpAtack");
                        }
                    }
                }

                if (m_canMove)//here
                {
                    m_horizontal = Input.GetAxisRaw("Horizontal");
                    m_virtical = Input.GetAxisRaw("Vertical");

                    if (Input.GetButtonDown("Fire1") && m_grapple.GetHaveTarget)
                    {
                        if (!m_anim.GetBool("IsHooking"))
                        {
                            m_anim.SetBool("IsHooking", true);
                            m_status.DecrementMana(m_manaCostGrapple);
                        }
                        else if (m_anim.GetBool("IsHooking"))
                        {
                            m_anim.SetBool("IsHooking", false);
                            m_grapple.Grapple();
                        }
                    }

                    if (IsGround())
                    {
                        if (Input.GetButtonDown("Jump") && !LockOnController.IsLock)
                        {
                            Jump();
                        }

                        if (Input.GetButtonDown("Sprint"))
                        {
                            Debug.Log("SprintButtonPushed");
                            if (!IsSprint) { m_rb.velocity = Vector3.zero; IsSprint = true; }
                            else IsSprint = false;
                        }

                        if (Input.GetButtonDown("Dodge") && m_status.Mana >= m_manaCostDodge)
                        {
                            m_status.DecrementMana(m_manaCostDodge);
                            if (m_dir == Vector3.zero)
                            {
                                m_anim.SetTrigger("Guard");
                            }
                            else
                            {
                                m_anim.SetTrigger("Dodge");
                            }
                        }
                    }
                }
                else if (!m_canMove && !m_isDodging)
                {
                    m_horizontal = 0f;
                    m_virtical = 0f;
                }
                break;
            case PlayerStates.OpenUi:
                break;
            default:
                break;
        }
        #endregion

        #region ベクトルに関する処理
        m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;

        if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
        else if (!LockOnController.IsLock)
        {
            /*ロックオン状態でなければ普通に動く*/

            m_dir = Camera.main.transform.TransformDirection(m_dir);
            m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            Quaternion targetRotation = Quaternion.LookRotation(m_dir);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpdTemp);
        }
        else if (LockOnController.IsLock && m_loc.GetTarget != null)
        {
            m_dir = this.transform.TransformDirection(m_dir);
            m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            Vector3 lookAtTarget = m_loc.GetTarget.transform.position;
            lookAtTarget.y = this.transform.position.y;
            this.transform.LookAt(lookAtTarget);/*敵を見ながら動く*/
        }
        #endregion
        #region　速さに関する処理
        if (m_canMove)
        {
            if (LockOnController.IsLock)
            {
                if (!m_isDodging) m_spdTemp = m_lockOnMoveSpeed;
            }
            else
            {
                if (IsGround())
                {
                    m_turnSpdTemp = m_turnSpeed;
                    if (IsSprint) m_spdTemp = m_sprintSpeed;
                    else if (!IsSprint && !m_isDodging) m_spdTemp = m_defaultMovingSpeed;
                }
                else
                {
                    m_turnSpdTemp *= 0.8f;
                    m_spdTemp = 5f;
                }
            }
        }
        else if (!m_canMove && m_isDodging)
        {
            if (!LockOnController.IsLock)
            {
                m_spdTemp = m_dodgeSpeed;
            }
            else
            {
                m_spdTemp = m_dodgeSpeed * 0.8f;
            }
        }

        Vector3 velo = m_dir.normalized * m_spdTemp; // 入力した方向に移動する


        if (m_rb.velocity.magnitude > m_maxSpeed)
        {
            m_maxSpeed = m_rb.velocity.magnitude;
        }

        if (m_debugSpdText) m_debugSpdText.text = $"Velo:{m_rb.velocity.magnitude.ToString("F1")}m/s\nMaxSpeed:{m_maxSpeed}";

        /*グラップル中あまりにも速度が早いため、y方向の速度も制限する*/
        if (m_rb.velocity.magnitude > m_limitSpeed)
        {
            if (m_debugSpdText) m_debugSpdText.color = Color.red;
            m_spdTemp = 0;
            if (m_rb.velocity.y > 0)
            {
                velo.y = 15;
            }
            else
            {
                velo.y = m_rb.velocity.y;
            }
        }
        else if (m_rb.velocity.magnitude < m_limitSpeed)
        {
            if (m_debugSpdText) m_debugSpdText.color = Color.blue;
            velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
        }
        //else if (m_rb.velocity.y > 10) //無駄かもしれない
        //{
        //    m_spdTemp = 0f;
        //    m_rb.velocity *= 0.95f;
        //    velo.y = 1;
        //    Debug.Log("m_rb.velocity.y。速度超過" + m_rb.velocity.y + m_spdTemp);
        //}
        //velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
        Debug.Log("m_rb.velocity.y" + m_rb.velocity.y + "  " + m_spdTemp);
        m_rb.velocity = velo;   // 計算した速度ベクトルをセットする
        #endregion
        #region アニメーションに関する処理
        if (m_anim)
        {
            if (IsGround())
            {
                Debug.Log($"接地している{m_hit.collider.name}");
                m_anim.SetBool("IsInTheAir", false);
                m_anim.ResetTrigger("JumpAtack");

                if (m_anim.GetBool("IsJumping")) m_anim.SetBool("IsJumping", false);
                if (m_anim.GetBool("IsFall"))//here
                {
                    m_anim.SetBool("IsFall", false);
                }
            }
            else
            {
                Debug.Log($"接地していない");
                m_anim.SetBool("IsInTheAir", true);
                m_anim.SetFloat("spd", 0f);
                if (m_rb.velocity.y > 0)
                {
                    if (!m_anim.GetBool("IsJumping")) m_anim.SetBool("IsJumping", true);
                    if (m_anim.GetBool("IsFall")) m_anim.SetBool("IsFall", false);
                }
                /*落下中は落ちるモーションを取る*/
                if (m_rb.velocity.y < 0)
                {
                    if (m_anim.GetBool("IsJumping")) m_anim.SetBool("IsJumping", false);
                    if (!m_anim.GetBool("IsFall")) m_anim.SetBool("IsFall", true);
                }
            }


            if (!LockOnController.IsLock)
            {
                m_anim.SetBool("isLockOn", false);
                Vector3 veloLockOn = m_rb.velocity;
                veloLockOn.y = 0f;
                m_anim.SetFloat("spd", veloLockOn.magnitude);//ジャンプ中に走ったりするのでy=落下速度は0にしている。
                m_anim.SetInteger("LockOnMotion", 0);
            }
            else
            {
                Debug.Log(PlayerState.m_PlayerDirState.ToString());
                m_anim.SetBool("isLockOn", true);
                if (IsSprint) IsSprint = false;
                if (IsGround())
                {
                    switch (PlayerState.m_PlayerDirState)
                    {
                        case PlayerMovingDirection.Neutral:
                            m_anim.SetFloat("PlayerDir", 0f);
                            break;
                        case PlayerMovingDirection.Forward:
                            m_anim.SetFloat("PlayerDir", 1f);
                            break;
                        case PlayerMovingDirection.Right:
                            m_anim.SetFloat("PlayerDir", 3f);
                            break;
                        case PlayerMovingDirection.Left:
                            m_anim.SetFloat("PlayerDir", 4f);
                            break;
                        case PlayerMovingDirection.Back:
                            m_anim.SetFloat("PlayerDir", 2f);
                            break;
                        default:
                            break;
                    }
                }
            }

        }
        #endregion
    }



    public void TestButton()
    {
        m_rb.constraints = RigidbodyConstraints.FreezePositionY;
        m_rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Hit()
    {

    }

    public void StartGuard()
    {
        m_guardCol.gameObject.SetActive(true);
        m_hitCol.gameObject.SetActive(false);
    }

    public void EndGuard()
    {
        m_guardCol.gameObject.SetActive(false);
        m_hitCol.gameObject.SetActive(true);
    }

    public void StartDodge()
    {
        m_isDodging = true;
        m_canMove = false;
        m_hitCol.gameObject.SetActive(false);
        m_anim.SetBool("IsDodging", m_isDodging);
        Debug.Log($"m_isDodging:{m_isDodging}::m_canmove:{m_canMove}");
    }

    public void EndDodge()
    {
        m_isDodging = false;
        m_canMove = true;
        m_hitCol.gameObject.SetActive(true);
        m_anim.SetBool("IsDodging", m_isDodging);
        Debug.Log($"m_isDodging:{m_isDodging}::m_canmove:{m_canMove}");
    }

    public void BeginAtack()
    {
        m_atackCol.gameObject.SetActive(true);
    }

    public void BeginUpperAtack()
    {
        m_upperAtackCol.gameObject.SetActive(true);
    }

    public void BeginKnockOffAtack()
    {
        m_upperKnockOffCol.gameObject.SetActive(true);
    }

    public void EndAtack()
    {
        if (m_atackCol.gameObject.activeSelf) m_atackCol.gameObject.SetActive(false);
        if (m_upperAtackCol.gameObject.activeSelf) m_upperAtackCol.gameObject.SetActive(false);
        if (m_upperKnockOffCol.gameObject.activeSelf) m_upperKnockOffCol.gameObject.SetActive(false);
    }

    public void CanMove()
    {
        m_canMove = true;
        m_turnSpdTemp = m_turnSpeed;
        m_rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        Debug.Log("CanMoveCalled" + m_canMove);
    }

    public void CanNotMove()
    {
        m_canMove = false;
        if (m_anim) m_anim.SetFloat("spd", 0f);
        m_turnSpdTemp = 0;
        if (!m_isDodging) { m_spdTemp = 0; Debug.Log("spdを0にしました"); }
        if (!IsGround()) m_rb.constraints = RigidbodyConstraints.FreezeAll;
        Debug.Log("CanNotMoveCalled" + m_canMove);
    }
    public void Jump()
    {
        if (m_anim)
        {
            m_anim.SetBool("IsJumping", true);
            m_anim.SetFloat("spd", 0f);
        }
        if (m_audioManager) m_audioManager.PlaySE("ジャンプ");
        m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
    }

    public void PlaySEOnAnimation(string name)
    {
        if (m_audioManager) m_audioManager.PlaySE(name);
    }

    private static RaycastHit m_hit;
    bool IsGround()
    {
        Ray ray = new Ray(this.transform.position + m_colider.center, Vector3.down);
        bool isGround = Physics.SphereCast(ray, m_sphereRadius, out m_hit, m_rayMaxDistance, m_groundMask);
        if (isGround) Debug.Log("Player::接地中");
        Debug.DrawRay(this.transform.position + m_colider.center, Vector3.down, Color.red);
        return isGround;
    }
    [SerializeField] bool isEnable = false;
    private void OnDrawGizmos()
    {
        if (isEnable == false)
            return;
        Gizmos.DrawWireSphere(this.transform.position + m_colider.center + Vector3.down * m_rayMaxDistance, m_sphereRadius);
    }
}
