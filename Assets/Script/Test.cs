using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの動きを決める。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Test : MonoBehaviour
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
    bool m_isAtacking;
    bool m_isDead;
    State m_currentState = State.Idle;
    State currentState
    {
        get => m_currentState;
        set
        {
            if (m_isDead) return;
            m_currentState = value;
        }
    }

    public enum State
    {
        Idle,
        Run,
        Sprint,
        Jump,
        Atack,
        LockOn,
        Dead,
    }

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
                        if (!IsSprint)
                        {// m_rb.velocity = Vector3.zero;
                            IsSprint = true;
                        }
                        else IsSprint = false;
                    }

                }
                break;
            case PlayerStates.OpenUi:
                break;
            default:
                break;
        }





        if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);


        Debug.Log("CurrentState:" + m_currentState);
        switch (m_currentState)
        {
            case State.Idle:
                m_simpleAnim.Stop("Atack");
                if (m_horizontal != 0 || m_virtical != 0)
                {
                    Run();
                }
                if (Input.GetButtonDown("Fire1"))
                {
                    Atack();
                }
                if (Input.GetButtonDown("Jump"))
                {
                    Jump();
                }
                break;
            case State.Run:
                NormalMove();
                m_simpleAnim.Stop("Atack");
                if (m_horizontal == 0 && m_virtical == 0)
                {
                    Idle();
                }
                if (Input.GetButtonDown("Jump"))
                {
                    Jump();
                }
                if (Input.GetButtonDown("Fire1"))
                {
                    Atack();
                }
                break;
            case State.Sprint:

                break;
            case State.Jump:
                m_simpleAnim.CrossFade("JumpMidAir", 0.1f);
                if (m_rb.velocity.y == 0) Idle();
                break;
            case State.Atack:
                m_rb.velocity = Vector3.zero;
                if (!m_simpleAnim.IsPlaying("Atack"))
                {
                    m_simpleAnim.CrossFade("Default", 0.2f);
                    m_currentState = State.Idle;
                }

                break;
            case State.LockOn:
                break;
            case State.Dead:
                break;
            default:
                break;
        }



    }

    private void LockOnMove()
    {
        m_dir = this.transform.TransformDirection(m_dir);
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

        Vector3 lookAtTarget = m_loc.GetTarget.transform.position;
        lookAtTarget.y = this.transform.position.y;
        this.transform.LookAt(lookAtTarget);/*敵を見ながら動く*/
    }

    private void NormalMove()
    {
        if (!IsGround()) return;
        /*ロックオン状態でなければ普通に動く*/
        m_dir = Camera.main.transform.TransformDirection(m_dir);
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする
        Quaternion targetRotation = Quaternion.LookRotation(m_dir);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
        m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
        m_simpleAnim.CrossFade("Run", 0.1f);
    }

    void Idle()
    {
        m_simpleAnim.CrossFade("Default", 0.1f);
        m_currentState = State.Idle;
    }

    void Run()
    {
        m_simpleAnim.CrossFade("Run", 0.1f);
        m_currentState = State.Run;
    }

    public void Atack()
    {
        Debug.Log("ATack::test");

        m_simpleAnim.CrossFade("Atack", 0.1f);

        m_currentState = State.Atack;
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
        if (!IsGround()) return;
        if (m_currentState == State.Jump) return;
        m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
        m_simpleAnim.CrossFade("JumpStart", 0.1f);
        m_currentState = State.Jump;
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
