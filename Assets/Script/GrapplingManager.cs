using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GrapplingManager : MonoBehaviour
{

    /// <summary>ボタンを押すと、jointの制限距離を短くする </summary>
    [SerializeField] float m_limit = 1f;
    /// <summary>プレイヤーとターゲットの距離がこの距離以下になるとターゲットは減速する</summary>
    [SerializeField] float m_disToDecelerateTarget = 2f;
    /// <summary>ターゲットを減速させるときにかける数</summary>
    [SerializeField] float m_decelerateSpeed = 0.5f;
    [SerializeField] float m_pullTime = 0.2f;

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
    }

    private void Update()
    {
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
            DrawLine(this.transform.position, m_lockOn.GetTarget.transform.position);
            m_timer += Time.deltaTime;
            if (m_timer > m_pullTime)
            {
                m_timer = 0;
                PullTarget(m_joint, m_lockOn.GetTarget);
            }

            if (Vector3.Distance(this.transform.position, m_lockOn.GetTarget.transform.position) <= m_disToDecelerateTarget)
            {
                m_targetRb.velocity = Vector3.zero;
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
        joint.connectedBody = null;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
    }

    void PullTarget(ConfigurableJoint joint, GameObject target)
    {
        var cj = joint.linearLimit;
        cj.limit = m_limit;
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
