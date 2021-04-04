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
    [SerializeField] float m_movingSpeed = 5f;
    [SerializeField] float m_turnSpeed = 5f;
    [SerializeField] float m_jumpPower = 5f;
    [SerializeField] float m_lockOnMovingSpeed = 4f;
    float m_spdTemp;
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
        m_spdTemp = m_movingSpeed;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = Vector3.forward * v + Vector3.right * h;
        //// カメラを基準に入力が上下=奥/手前, 左右=左右にキャラクターを向ける
        dir = Camera.main.transform.TransformDirection(dir);    // メインカメラを基準に入力方向のベクトルを変換する
        dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

        if (IsGround())
        {
            Debug.Log($"接地している{m_hit.collider.name}");
            if (dir == Vector3.zero)
            {
                m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
            }
            else if (!LockOnController.IsLock)
            {
                /*ロックオン状態でなければ普通に動く*/
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント
            }
            else
            {
                /*敵を見ながら動く*/
                this.transform.LookAt(m_loc.GetTarget.transform.position);
            }

            //here
            Vector3 velo = dir.normalized * m_movingSpeed; // 入力した方向に移動する
            velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
            m_rb.velocity = velo;   // 計算した速度ベクトルをセットする

            if (Input.GetButtonDown("Jump"))
            {
                //m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
                /*とりあえずなにもしない*/
            }
        }
        else
        {
            Debug.Log($"接地していない");
        }

        if (m_anim)
        {
            if (!LockOnController.IsLock)
            {
                m_movingSpeed = m_spdTemp;
                m_anim.SetFloat("spd", m_rb.velocity.magnitude);
                m_anim.SetInteger("LockOnMotion", 0);
            }
            else
            {
                Debug.Log(m_movingSpeed.ToString());
                if ((h == 0 && v == 0) || v > 0)
                {
                    m_movingSpeed = m_spdTemp;
                    m_anim.SetFloat("spd", m_rb.velocity.magnitude);
                    m_anim.SetInteger("LockOnMotion", 0);
                }
                if (v < 0)
                {
                    //後退
                    m_movingSpeed = m_lockOnMovingSpeed;
                    m_anim.SetInteger("LockOnMotion", 1);
                }
                if (v == 0)
                {
                    if (h > 0)
                    {
                        //右歩き
                        m_movingSpeed = m_lockOnMovingSpeed;
                        m_anim.SetInteger("LockOnMotion", 2);
                    }
                    else if (h < 0)
                    {
                        m_movingSpeed = m_lockOnMovingSpeed;
                        m_anim.SetInteger("LockOnMotion", 3);
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
