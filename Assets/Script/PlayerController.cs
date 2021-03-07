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
    /// <summary>接地判定に関するフィールド</summary>
    [SerializeField] float m_sphereRadius = 1f;
    [SerializeField] float m_rayMaxDistance = 1f;
    [SerializeField] LayerMask m_groundMask;
    /// <summary>ロックオンに関するフィールド</summary>
    LockOnController m_loc;

    Rigidbody m_rb;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_loc = FindObjectOfType<LockOnController>();
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
                /*ロックオン状態なら敵を見ながら動く*/
                this.transform.LookAt(m_loc.GetTarget.transform.position);
            }

            Vector3 velo = dir.normalized * m_movingSpeed; // 入力した方向に移動する
            velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
            m_rb.velocity = velo;   // 計算した速度ベクトルをセットする

            if (Input.GetButtonDown("Jump"))
            {
                m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);

            }
        }

    }

    bool IsGround()
    {
        Ray ray = new Ray(this.transform.position, Vector3.down);

        bool isGround = Physics.SphereCast(ray, m_sphereRadius, m_rayMaxDistance, m_groundMask);


        return isGround;
    }

    private void OnDrawGizmos()
    {
        Vector3 start = this.transform.position;   // start: オブジェクトの中心
        Vector3 end = start + Vector3.down * m_rayMaxDistance;  // end: start から真下の地点


        Gizmos.DrawSphere(end, m_sphereRadius);
    }


}
