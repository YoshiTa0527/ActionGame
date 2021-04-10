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
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsHooked)
                {
                    IsHooked = true;
                    Hook(m_joint, m_lockOn.GetTarget);
                }
                else IsHooked = false;
            }
        }
        else
        {
            IsHooked = false;
        }

        if (IsHooked)
        {
            DrawLine(m_sourcePos.position, m_lockOn.GetTarget.transform.position);
            m_timer += Time.deltaTime;
            if (m_timer > m_pullTime)
            {
                m_timer = 0;
                PullTarget(m_joint, m_lockOn.GetTarget);
            }
            //StartCoroutine(PullTargetCo(m_joint, m_lockOn.GetTarget, m_pullTime));

            if (Vector3.Distance(this.transform.position, m_lockOn.GetTarget.transform.position) <= m_disToDecelerateTarget)
            {
                if (m_lockOn.GetTarget.tag == "Enemy")
                {
                    m_targetRb = m_lockOn.GetTarget.GetComponent<Rigidbody>();
                    m_targetRb.velocity = Vector3.zero;
                }
                else
                {
                    m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                    m_playerRb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
                    Debug.Log("Pull::Pushed Player");
                }
                IsHooked = false;
            }
        }
        else
        {
            LoseHook(m_joint);
            HideLine();
        }
    }

    void Hook(ConfigurableJoint joint, GameObject target)
    {
        m_targetRb = target.GetComponent<Rigidbody>();
        if (m_targetRb)
        {
            joint.connectedBody = m_targetRb;

            var cj = joint.linearLimit;
            m_playerRb = this.gameObject.GetComponent<Rigidbody>();
            if (target.tag == "Enemy")
            {
                m_playerRb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                joint.yMotion = ConfigurableJointMotion.Locked;
            }
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
        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
        m_playerRb.constraints = RigidbodyConstraints.None;
        m_playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        joint.connectedBody = null;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
    }

    void PullTarget(ConfigurableJoint joint, GameObject target)
    {
        var cj = joint.linearLimit;

        cj.limit = m_pullLimit;
        joint.linearLimit = cj;

        //joint.xMotion = ConfigurableJointMotion.Locked;
        //joint.yMotion = ConfigurableJointMotion.Locked;
        //joint.zMotion = ConfigurableJointMotion.Locked;

    }

    IEnumerator PullTargetCo(ConfigurableJoint joint, GameObject target, float time)
    {
        var cj = joint.linearLimit;
        yield return new WaitForSeconds(time);
        if (target.tag == "Enemy")
        {
            m_playerRb = this.gameObject.GetComponent<Rigidbody>();
            m_playerRb.constraints = RigidbodyConstraints.FreezeAll;
        }

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
}
