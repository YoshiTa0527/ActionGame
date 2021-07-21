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
    [SerializeField] float m_defaultMovingSpeed = 5f;
    [SerializeField] float m_sprintSpeed = 8f;
    [SerializeField] float m_lockOnMoveSpeed = 3f;
    [SerializeField] float m_turnSpeed = 5f;
    [SerializeField] float m_jumpPower = 5f;
    [SerializeField] float m_limitSpeed = 20f;
    [SerializeField] float m_decelerateSpeed = 1.1f;
    [SerializeField] float m_landingMotionDis = 3f;
    [SerializeField] float m_fallMotionDis = 1f;
    [SerializeField] float m_movingPowerInTheAir = 2f;
    float m_highestPos;
    float m_spdTemp;
    float m_turnSpdTemp;
    bool m_canmove = true;
    bool m_isDodging = false;
    int m_jumpAtackCounter;
    Vector3 m_currentDir;
    static public bool IsSprint { get; set; }
    /// <summary>接地判定に関するフィールド</summary>
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_colider;
    /// <summary>ロックオンに関するフィールド</summary>
    LockOnController m_loc;
    /// <summary>攻撃判定に使うコライダー</summary>
    [SerializeField] Collider m_atackCol;
    [SerializeField] Collider m_upperAtackCol;
    [SerializeField] Collider m_upperKnockOffCol;
    Rigidbody m_rb;
    Animator m_anim;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_loc = FindObjectOfType<LockOnController>();
        m_colider = GetComponent<CapsuleCollider>();
        m_anim = GetComponent<Animator>();
        m_spdTemp = m_defaultMovingSpeed;
        m_turnSpdTemp = m_turnSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 velo = m_dir.normalized * m_spdTemp; // 入力した方向に移動する
        velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
        m_rb.velocity = velo;   // 計算した速度ベクトルをセットする
    }
    private void Update()
    {
        if (m_rb.velocity.magnitude > m_limitSpeed)
        {
            Debug.Log("はやすぎ");
            m_rb.velocity = new Vector3(m_rb.velocity.x / m_decelerateSpeed, m_rb.velocity.y, m_rb.velocity.z / m_decelerateSpeed);
        }

        switch (PlayerState.m_PlayerStates)
        {
            case PlayerStates.InGame:
                if (Input.GetButtonDown("Atack"))
                {
                    if (IsGround())
                    {
                        m_anim.SetTrigger("Atack1");
                        m_jumpAtackCounter = 0;
                    }
                    else
                    {
                        m_jumpAtackCounter++;
                        if (m_jumpAtackCounter <= 1)
                        {
                            m_anim.SetTrigger("JumpAtack");
                        }
                    }
                    Debug.Log("AtackButtonPushed");
                }
                if (m_canmove)//here
                {

                    m_horizontal = Input.GetAxisRaw("Horizontal");
                    m_virtical = Input.GetAxisRaw("Vertical");

                    if (Input.GetButtonDown("Sprint"))
                    {
                        Debug.Log("SprintButtonPushed");
                        if (!IsSprint) { m_rb.velocity = Vector3.zero; IsSprint = true; }
                        else IsSprint = false;
                    }

                    if (Input.GetButtonDown("Dodge"))
                    {
                        m_anim.SetTrigger("Dodge");
                    }
                }
                else
                {
                    m_horizontal = 0f;
                    m_virtical = 0f;
                    m_rb.velocity = Vector3.zero;
                }
                break;
            case PlayerStates.OpenUi:
                break;
            default:
                break;
        }

        //if (m_horizontal != 0 && m_virtical != 0 && !m_isDodging)
        //{
        //    m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
        //}
        //else if (m_horizontal == 0 && m_virtical == 0 && m_isDodging)
        //{
        //    m_dir = Vector3.forward * 1;
        //}
        //else
        //{
        //    m_dir = Vector3.zero;
        //}

        if (!m_isDodging) m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
       


        if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
        else if (!LockOnController.IsLock)
        {
            /*ロックオン状態でなければ普通に動く*/

            m_dir = Camera.main.transform.TransformDirection(m_dir);
            m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            Quaternion targetRotation = Quaternion.LookRotation(m_dir);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
        }
        else
        {
            if (!m_isDodging)
            {
                m_dir = this.transform.TransformDirection(m_dir);
                m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

                Vector3 lookAtTarget = m_loc.GetTarget.transform.position;
                lookAtTarget.y = this.transform.position.y;
                this.transform.LookAt(lookAtTarget);/*敵を見ながら動く*/
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(m_dir);
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
            }
        }

        /*ロックオン中、スプリント中で速度を変える*/
        if (LockOnController.IsLock)
        {
            m_spdTemp = m_lockOnMoveSpeed;
        }

        else
        {
            if (IsGround())
            {
                if (IsSprint) m_spdTemp = m_sprintSpeed;
                else if (!IsSprint && !m_isDodging) m_spdTemp = m_defaultMovingSpeed;
                //else if (m_isDodging) m_spdTemp = m_sprintSpeed;
            }
            else
            {
                m_spdTemp = 2.5f;
            }
        }

        if (m_anim)
        {
            if (IsGround())
            {
                Debug.Log($"接地している{m_hit.collider.name}");
                m_anim.SetBool("IsInTheAir", false);
                m_anim.ResetTrigger("JumpAtack");
                if (Input.GetButtonDown("Jump") && m_canmove)
                {
                    Jump();
                }

                // if (m_anim.GetBool("IsJumping")) m_anim.SetBool("IsJumping", false);
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
                Vector3 velo = m_rb.velocity;
                velo.y = 0f;
                m_anim.SetFloat("spd", velo.magnitude);//ジャンプ中に走ったりするのでy=落下速度は0にしている。
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
    }

    public void TestDodge()
    {
        
    }

    public void StartDodge()
    {
        m_isDodging = true;
        if (m_dir != Vector3.zero)
        {

        }
        else if (m_dir == Vector3.zero)
        {

        }
        m_anim.SetBool("IsDodging", m_isDodging);
    }

    public void EndDodge()
    {
        m_isDodging = false;
        m_anim.SetBool("IsDodging", m_isDodging);
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
        m_canmove = true;
        m_turnSpeed = m_turnSpdTemp;
        m_rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        Debug.Log("CanMoveCalled" + m_canmove);
    }

    public void CanNotMove()
    {
        m_canmove = false;
        if (m_anim) m_anim.SetFloat("spd", 0f);
        m_turnSpeed = 0;
        m_rb.velocity = new Vector3(0f, 0f, 0f);

        //m_rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        Debug.Log("CanNotMoveCalled" + m_canmove);
    }
    public void Jump()
    {
        if (m_anim)
        {
            m_anim.SetBool("IsJumping", true);
            m_anim.SetFloat("spd", 0f);
        }
        m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
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
