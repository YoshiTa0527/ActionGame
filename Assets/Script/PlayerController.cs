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
    [SerializeField] float m_landingMotionDisForRay = 3f;
    [SerializeField] float m_fallMotionDisForRay = 1f;
    float m_highestPos;
    float m_spdTemp;
    float m_turnSpdTemp;
    bool m_canmove = true;
    bool m_isAtacking;
    static public bool IsSprint { get; set; }
    /// <summary>接地判定に関するフィールド</summary>
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    CapsuleCollider m_colider;
    /// <summary>ロックオンに関するフィールド</summary>
    LockOnController m_loc;
    Rigidbody m_rb;

    SimpleAnimation m_simpleAnim;
    bool m_isInTheAir;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_loc = FindObjectOfType<LockOnController>();
        m_colider = GetComponent<CapsuleCollider>();
        m_simpleAnim = GetComponent<SimpleAnimation>();
        m_spdTemp = m_defaultMovingSpeed;
        m_turnSpdTemp = m_turnSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 velo = m_dir.normalized * m_spdTemp; // 入力した方向に移動する
        if (!IsGround()) velo = m_dir.normalized * m_spdTemp / 2;
        velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
        m_rb.velocity = velo;   // 計算した速度ベクトルをセットする
    }
    private void Update()
    {
        if (m_rb.velocity.magnitude > m_limitSpeed)
        {
            Debug.Log("はやすぎ");
            m_rb.velocity = new Vector3(m_rb.velocity.x / m_decelerateSpeed, m_rb.velocity.y / m_decelerateSpeed, m_rb.velocity.z / m_decelerateSpeed);
        }

        switch (PlayerState.m_PlayerStates)
        {
            case PlayerStates.InGame:
                if (m_canmove)//here
                {
                    m_horizontal = Input.GetAxisRaw("Horizontal");
                    m_virtical = Input.GetAxisRaw("Vertical");
                    if (Input.GetButtonDown("Sprint"))
                    {
                        Debug.Log("SprintButtonPushed");
                        if (!IsSprint && !LockOnController.IsLock)
                        {// m_rb.velocity = Vector3.zero;
                            IsSprint = true;
                        }
                        else IsSprint = false;
                    }

                    if (Input.GetButtonDown("Atack"))
                    {
                        m_isAtacking = true;
                    }

                }
                break;
            case PlayerStates.OpenUi:
                break;
            default:
                break;
        }


        m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
        if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
        else if (LockOnController.IsLock)
        {
            /*ロックオン状態だと敵を見ながら動く*/
            LockOnMoveUpdate();
        }
        else
        {
            NormalMoveUpdate();
        }

        if (IsGround())
        {
            Debug.Log($"接地している:{m_isInTheAir}");
            m_isInTheAir = false;

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }
        else
        {
            Debug.Log($"接地していない");
            m_isInTheAir = true;
        }

        /*simpleAnimationに関する処理*/
        if (m_simpleAnim)
        {
            if (!IsGround())
            {
                if (m_rb.velocity.y > 0) m_simpleAnim.CrossFade("JumpStart", 0.1f);
                else if (m_rb.velocity.y < 0) m_simpleAnim.CrossFade("JumpMidAir", 0.1f);
            }
            else
            {
                //if (m_horizontal != 0 || m_virtical != 0)
                //{
                //    if (IsSprint) m_simpleAnim.CrossFade("Sprint", 0.1f);
                //    else m_simpleAnim.CrossFade("Run", 0.1f);

                //    if (LockOnController.IsLock)
                //    {
                //        Debug.Log("居ずロック");
                //        switch (PlayerState.m_PlayerDirState)
                //        {
                //            case PlayerMovingDirection.Neutral:
                //                m_simpleAnim.CrossFade("Default", 0.1f);
                //                break;
                //            case PlayerMovingDirection.Forward:
                //                m_simpleAnim.CrossFade("WalkForward", 0.1f);
                //                Debug.Log("Forward");
                //                break;
                //            case PlayerMovingDirection.Right:
                //                m_simpleAnim.CrossFade("WalkRight", 0.1f);
                //                break;
                //            case PlayerMovingDirection.Left:
                //                m_simpleAnim.CrossFade("WalkLeft", 0.1f);
                //                break;
                //            case PlayerMovingDirection.Back:
                //                m_simpleAnim.CrossFade("WalkBack", 0.1f);
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
                //else if (m_horizontal == 0 && m_virtical == 0) m_simpleAnim.CrossFade("Default", 0.1f);

                Debug.Log($"再生中{m_simpleAnim.IsPlaying("Atack")}");

                switch (PlayerState.m_PlayerDirState)
                {
                    case MovingDirection.Neutral:
                        if (m_isAtacking)
                        {
                            m_simpleAnim.CrossFade("Atack", 0.1f);

                        }
                        else m_simpleAnim.CrossFade("Default", 0.1f);
                        break;
                    case MovingDirection.Move:
                        if (IsSprint) m_simpleAnim.CrossFade("Sprint", 0.1f);
                        else m_simpleAnim.CrossFade("Run", 0.1f);
                        break;
                    case MovingDirection.Forward:
                        m_simpleAnim.CrossFade("WalkForward", 0.1f);
                        break;
                    case MovingDirection.Right:
                        m_simpleAnim.CrossFade("WalkRight", 0.1f);
                        break;
                    case MovingDirection.Left:
                        m_simpleAnim.CrossFade("WalkLeft", 0.1f);
                        break;
                    case MovingDirection.Back:
                        m_simpleAnim.CrossFade("WalkBack", 0.1f);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void EndAtack()
    {
        m_isAtacking = false;
    }

    private void LockOnMoveUpdate()
    {
        m_spdTemp = m_lockOnMoveSpeed;
        Vector3 lookAtTarget = m_loc.GetTarget.transform.position;
        m_dir = this.transform.TransformDirection(m_dir);
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする
        lookAtTarget.y = this.transform.position.y;
        this.transform.LookAt(lookAtTarget);/*敵を見ながら動く*/
    }

    private void NormalMoveUpdate()
    {
        if (IsSprint) { m_spdTemp = m_sprintSpeed; }
        else m_spdTemp = m_defaultMovingSpeed;

        m_dir = Camera.main.transform.TransformDirection(m_dir);
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする
        Quaternion targetRotation = Quaternion.LookRotation(m_dir);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
    }

    public void CanMove()
    {
        m_canmove = true;
        m_turnSpeed = m_turnSpdTemp;
        m_rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
        Debug.Log("CanMoveCalled");
    }

    public void CanNotMove()
    {
        m_canmove = false;

        m_turnSpeed = 0;
        m_rb.velocity = Vector3.zero;
        // m_rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        Debug.Log("CanNotMoveCalled");
    }
    public void Jump()
    {
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
