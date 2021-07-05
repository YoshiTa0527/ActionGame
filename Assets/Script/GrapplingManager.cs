using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class GrapplingManager : MonoBehaviour
{
    /// <summary>この位置からlineを描画する</summary>
    [SerializeField] Transform m_sourcePos;
    /// <summary>ボタンを押すと、jointの制限距離を短くする </summary>
    [SerializeField] float m_pullLimit = 1f;
    /// <summary>プレイヤーとターゲットの距離がこの距離以下になるとターゲットは減速する</summary>
    [SerializeField] float m_disToDecelerateTarget = 2f;
    /// <summary>ターゲットを減速させるときにかける数</summary>
    [SerializeField] float m_decelerateSpeed = 0.5f;
    [SerializeField] float m_pullTime = 0.2f;

    [SerializeField] bool isEnable;

    [SerializeField] InOutTracking m_iot;

    GameObject m_currentTarget;

    Rigidbody m_playerRb;
    Rigidbody m_targetRb;
    ConfigurableJoint m_joint;
    LockOnController m_lockOn;
    LineRenderer m_lineRend;
    float m_timer;

    public static bool IsHooked { get; set; }

    private void Start()
    {
        m_lineRend = GetComponent<LineRenderer>();
        m_joint = GetComponent<ConfigurableJoint>();
        m_lockOn = FindObjectOfType<LockOnController>();
        if (!m_sourcePos) m_sourcePos = this.transform;
        HideLine();
    }

    private void Update()
    {
        if (!isEnable) return;
        /*ロックオン中ならターゲットにフックをつける*/
        if (LockOnController.IsLock)
        {
            m_currentTarget = m_lockOn.GetTarget;
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsHooked)
                {
                    IsHooked = true;
                    Hook(m_joint, m_currentTarget);
                }
                else IsHooked = false;
            }
        }
        else if (!LockOnController.IsLock && m_iot.GetGrapplingTarget)
        {
            m_currentTarget = m_iot.GetGrapplingTarget.gameObject;
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsHooked)
                {
                    IsHooked = true;
                    Hook(m_joint, m_currentTarget);
                }
                else IsHooked = false;
            }
        }
        else IsHooked = false;

        if (IsHooked)
        {
            DrawLine(m_sourcePos.position, m_currentTarget.transform.position);
            if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget)
            {
                Debug.Log("Grapple::近づいた");
                if (m_currentTarget.tag == "Enemy")
                {
                    m_targetRb = m_currentTarget.GetComponent<Rigidbody>();
                    m_targetRb.velocity = Vector3.zero;
                    LoseHook(m_joint);
                    IsHooked = false;
                }
                else
                {
                    float yAbs = Mathf.Abs(m_currentTarget.transform.position.y - this.transform.position.y);
                    Debug.Log("Grapple::近づいた" + yAbs);
                    if (CalcValue(yAbs, 1f) && this.transform.position.y > m_currentTarget.transform.position.y)
                    {
                        Debug.Log("高さほぼ一緒");
                        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                        m_playerRb.velocity = Vector3.zero;
                        m_playerRb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                        LoseHook(m_joint);
                        IsHooked = false;
                    }

                }
            }
            m_timer += Time.deltaTime;
            if (m_timer > m_pullTime)
            {
                m_timer = 0;
                PullTarget(m_joint, m_currentTarget);
            }
        }
    }

    /// <summary>
    /// 指定された値xが指定された値thresholdの範囲内にある場合trueを返す
    /// </summary>
    /// <param name="x"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    bool CalcValue(float x, float threshold)
    {
        if (-1 * threshold < x && x < threshold) return true;
        else return false;
    }

    void Hook(ConfigurableJoint joint, GameObject target)
    {
        m_targetRb = target.GetComponent<Rigidbody>();
        if (m_targetRb)
        {
            joint.connectedBody = m_targetRb;
            var cj = joint.linearLimit;
            m_playerRb = this.gameObject.GetComponent<Rigidbody>();
            EnableFreezePos(this.gameObject);

            cj.limit = Vector3.Distance(this.transform.position, target.transform.position);
            joint.linearLimit = cj;

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

        }
        else
        {
            Debug.LogErrorFormat("{0} doesn't have Rigidbody.", target.name);
        }
    }

    void LoseHook(ConfigurableJoint joint)
    {
        Debug.Log("GrapplingManager::LoseHook()");
        HideLine();
        DisableFreezePos(this.gameObject);

        joint.connectedBody = null;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
    }

    void PullTarget(ConfigurableJoint joint, GameObject target)
    {
        DisableFreezePos(this.gameObject);
        var cj = joint.linearLimit;

        cj.limit = m_pullLimit;
        joint.linearLimit = cj;
    }

    void DrawLine(Vector3 source, Vector3 destination)
    {
        m_lineRend.SetPosition(0, source);
        m_lineRend.SetPosition(1, destination);
    }

    void HideLine()
    {
        DrawLine(Vector3.zero, Vector3.zero);
    }

    void EnableFreezePos(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    void DisableFreezePos(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
