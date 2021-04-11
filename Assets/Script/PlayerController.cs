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
    float m_spdTemp;
    static public bool IsSprint { get; set; }
    /// <summary>接地判定に関するフィールド</summary>
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>このコライダーを中心に設置判定のrayを出す</summary>
    [SerializeField] CapsuleCollider m_colider;
    /// <summary>ロックオンに関するフィールド</summary>
    LockOnController m_loc;
    Rigidbody m_rb;
    Animator m_anim;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_loc = FindObjectOfType<LockOnController>();
        m_colider = GetComponent<CapsuleCollider>();
        m_anim = GetComponent<Animator>();
        m_spdTemp = m_defaultMovingSpeed;
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
            m_rb.velocity = new Vector3(m_rb.velocity.x / m_decelerateSpeed, m_rb.velocity.y / m_decelerateSpeed, m_rb.velocity.z / m_decelerateSpeed);
        }
        switch (PlayerState.m_PlayerStates)
        {
            case PlayerStates.InGame:
                m_horizontal = Input.GetAxisRaw("Horizontal");
                m_virtical = Input.GetAxisRaw("Vertical");
                if (Input.GetButtonDown("Sprint"))
                {
                    Debug.Log("SprintButtonPushed");
                    if (!IsSprint) { m_rb.velocity = Vector3.zero; IsSprint = true; }
                    else IsSprint = false;
                }
                break;
            case PlayerStates.OpenUi:
                break;
            default:
                break;
        }

        m_dir = Vector3.forward * m_virtical + Vector3.right * m_horizontal;
        //// カメラを基準に入力が上下=奥/手前, 左右=左右にキャラクターを向ける
        m_dir = Camera.main.transform.TransformDirection(m_dir);    // メインカメラを基準に入力方向のベクトルを変換する
        m_dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

        /*ロックオン中、スプリント中で速度を変える*/
        if (LockOnController.IsLock) m_spdTemp = m_lockOnMoveSpeed;
        else
        {
            if (IsSprint) m_spdTemp = m_sprintSpeed;
            else m_spdTemp = m_defaultMovingSpeed;
        }

        if (IsGround())
        {
            Debug.Log($"接地している{m_hit.collider.name}");
            if (m_anim) m_anim.SetBool("isFall", false);

            if (m_dir == Vector3.zero) m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
            else if (!LockOnController.IsLock)
            {
                /*ロックオン状態でなければ普通に動く*/
                Quaternion targetRotation = Quaternion.LookRotation(m_dir);
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
            }
            else
            {
                Vector3 lookAtTarget = m_loc.GetTarget.transform.position;
                lookAtTarget.y = this.transform.position.y;
                this.transform.LookAt(lookAtTarget);/*敵を見ながら動く*/
            }

            if (Input.GetButtonDown("Jump"))
            {
                /*とりあえずなにもしない*/
            }
        }
        else
        {
            Debug.Log($"接地していない");
            if (m_anim) m_anim.SetBool("isFall", true);
        }

        if (m_anim)
        {
            if (!LockOnController.IsLock)
            {
                m_anim.SetBool("isLockOn", false);
                m_anim.SetFloat("spd", m_rb.velocity.magnitude);
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
                            m_anim.SetInteger("LockOnMotion", 0);
                            break;
                        case PlayerMovingDirection.Forward:
                            m_anim.SetInteger("LockOnMotion", 1);
                            break;
                        case PlayerMovingDirection.Right:
                            m_anim.SetInteger("LockOnMotion", 3);
                            break;
                        case PlayerMovingDirection.Left:
                            m_anim.SetInteger("LockOnMotion", 4);
                            break;
                        case PlayerMovingDirection.Back:
                            m_anim.SetInteger("LockOnMotion", 2);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    RaycastHit m_hit;
    bool IsGround()
    {
        Ray ray = new Ray(this.transform.position + m_colider.center, Vector3.down);
        bool isGround = Physics.SphereCast(ray, m_sphereRadius, out m_hit, m_rayMaxDistance, m_groundMask);
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
