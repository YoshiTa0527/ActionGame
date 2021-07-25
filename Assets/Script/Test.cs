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
    [SerializeField] GameObject m_target;
    [SerializeField] Text m_debugText;
    [SerializeField] float m_distance = 5f;
    [SerializeField] float m_addforcePower = 10f;
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
    bool m_isDodge = false;
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
    float m_timer;

    SimpleAnimation m_simpleAnim;
    bool m_isInTheAir;

    public void TestButton()
    {
        if (m_rb)
        {
            m_rb.constraints = RigidbodyConstraints.FreezeAll;
            Debug.Log("RigidBodyConstraints");
        }
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
    bool m_isNearTarget = false;
    private void Update()
    {
        if (m_rb.velocity.magnitude > m_limitSpeed)
        {
            Debug.Log("はやすぎ");
            m_rb.velocity = new Vector3(m_rb.velocity.x / m_decelerateSpeed, m_rb.velocity.y / m_decelerateSpeed, m_rb.velocity.z / m_decelerateSpeed);
        }

        if (Vector3.Distance(this.transform.position, m_target.transform.position) < m_distance && !m_isNearTarget)
        {
            m_debugText.color = Color.red;
            m_debugText.text = Vector3.Distance(this.transform.position, m_target.transform.position).ToString("F1") + "m";
            m_rb.AddForce(((m_target.transform.position - this.transform.position) + Vector3.up * 1.2f) * m_addforcePower, ForceMode.Impulse);
            m_isNearTarget = true;
            Debug.Log("near");
        }
        else
        {
            m_debugText.color = Color.blue;
            m_debugText.text = Vector3.Distance(this.transform.position, m_target.transform.position).ToString("F1") + "m";
            m_isNearTarget = false;
            Debug.Log("far");
        }

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
            if (Input.GetButtonDown("Dodge"))
            {
                if (!m_isDodge)
                {
                    m_isDodge = true;
                    Debug.Log("m_dodge:" + m_isDodge);
                    m_canmove = false;
                }
            }
        }
        else if (!m_canmove && !m_isDodge)
        {
            m_rb.velocity = Vector3.zero;
        }
        else if (!m_canmove && m_isDodge)
        {
            Vector3 dodgeVelo = m_dir.normalized * 10f;
            dodgeVelo.y = m_rb.velocity.y;
            m_rb.velocity = dodgeVelo;
        }

        if (m_isDodge)
        {
            m_timer += Time.deltaTime;
            if (m_timer > 2f)
            {
                m_timer = 0;
                m_isDodge = false;
                Debug.Log("m_dodge:" + m_isDodge);
                m_canmove = true;
            }
        }

        m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
        if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);

        m_dir = Camera.main.transform.TransformDirection(m_dir);
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

        Quaternion targetRotation = Quaternion.LookRotation(m_dir);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント


        Vector3 velo = m_dir.normalized * m_spdTemp; // 入力した方向に移動する
        if (!IsGround()) velo = m_dir.normalized * m_spdTemp / 2;
        velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
        m_rb.velocity = velo;   // 計算した速度ベクトルをセットする


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
