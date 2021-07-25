using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class NewGrapplingManager : MonoBehaviour
{
    /// <summary>デバッグ用のテキスト。ターゲットを表示する</summary>
    [SerializeField] Text m_targetText = default;
    [SerializeField] Text m_stateText = default;
    /// <summary>この位置からlineを描画する</summary>
    [SerializeField] Transform m_sourcePos;
    /// <summary>ボタンを押すと、jointの制限距離を短くする </summary>
    [SerializeField] float m_pullLimit = 1f;
    /// <summary>プレイヤーとターゲットの距離がこの距離以下になるとターゲットは減速する</summary>
    [SerializeField] float m_disToDecelerateTarget = 2f;
    /// <summary>ターゲットを減速させるときにかける数</summary>
    [SerializeField] float m_decelerateSpeed = 0.5f;
    [SerializeField] float m_pullTime = 0.2f;
    /// <summary>直前のターゲット</summary>
    GameObject m_latestTarget;
    [SerializeField] bool isEnable;
    bool m_haveTarget;
    public bool GetHaveTarget { get => m_haveTarget; }
    bool m_isDrawingLine;

    [SerializeField] InOutTracking m_iot;

    GameObject m_currentTarget = null;

    Rigidbody m_playerRb;
    Rigidbody m_targetRb;
    ConfigurableJoint m_joint;
    LockOnController m_lockOn;
    LineRenderer m_lineRend;
    float m_timer;

    public static bool IsHooked { get; set; }
    Animator m_anim;

    private void Start()
    {
        m_lineRend = GetComponent<LineRenderer>();
        m_joint = GetComponent<ConfigurableJoint>();
        m_lockOn = FindObjectOfType<LockOnController>();
        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();
        if (!m_sourcePos) m_sourcePos = this.transform;
        HideLine();
    }

    private void Update()
    {
        if (!isEnable) return;
        if (m_currentTarget)
        {
            m_targetText.text = $"Target:{m_currentTarget.name}::{m_haveTarget}";
            m_haveTarget = true;
        }
        else
        {
            m_targetText.text = $"Target:{m_currentTarget}::{m_haveTarget}";
            m_haveTarget = false;
        }





        if (IsHooked)
        {
            m_timer += Time.deltaTime;
            if (m_timer > m_pullTime)
            {
                m_timer = 0;
                // PullTarget(m_joint, m_currentTarget);
            }

            if (m_currentTarget.tag == "Enemy")
            {
                EnemyController enemyController = m_currentTarget.GetComponent<EnemyController>();
                if (enemyController.EnemyIsGround)
                {
                    EnableConstraints(this.gameObject);
                    m_targetRb = m_currentTarget.GetComponent<Rigidbody>();
                    m_targetRb.velocity = Vector3.zero;
                    if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget)
                    {
                        Debug.Log("敵に近づいた");
                        LoseHook(m_joint);
                    }
                }
                else
                {
                    EnableConstraints(m_currentTarget);
                    DisableConstraints(this.gameObject);
                    float yAbs = Mathf.Abs(m_currentTarget.transform.position.y - this.transform.position.y);
                    if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget
                        && this.transform.position.y >= m_currentTarget.transform.position.y)
                    {
                        Debug.Log("高さほぼ一緒");
                        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                        m_playerRb.velocity = Vector3.zero;
                        // m_playerRb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                        LoseHook(m_joint);
                    }
                }
            }
            else if (m_currentTarget.tag == "GrapplePoint")
            {
                DisableConstraints(this.gameObject);
                float yAbs = Mathf.Abs(m_currentTarget.transform.position.y - this.transform.position.y);
                //if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget //&& CalcValue(yAbs, 5f)
                //    && this.transform.position.y > m_currentTarget.transform.position.y)
                //{
                //    Debug.Log("Grapple::十分近づいた");
                //    m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                //    //m_playerRb.velocity = Vector3.zero;
                //    //m_playerRb.AddForce((m_currentTarget.transform.position - this.transform.position + new Vector3(0, 0.5f, 0)) * 4.5f, ForceMode.Impulse);
                //    Debug.Log("Grapple::アドフォース！");
                //    LoseHook(m_joint);
                //}

                if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget)
                {
                    Debug.Log("Grapple::近づいた。yAbs=" + yAbs);
                    if (this.transform.position.y < m_currentTarget.transform.position.y)
                    {
                        LoseHook(m_joint);
                        //m_playerRb.AddForce((this.transform.forward + new Vector3(0, 1, 0) / 5) * 10, ForceMode.Impulse);

                        Debug.Log("Grapple::下にある");
                    }
                }
            }

            if (m_isDrawingLine) DrawLine(m_sourcePos.position, m_currentTarget.transform.position);
        }
        else
        {
            //if (m_latestTarget)
            //{
            //    if (Vector3.Distance(this.transform.position, m_latestTarget.transform.position) <= m_disToDecelerateTarget)
            //    {
            //        if (this.transform.position.y > m_latestTarget.transform.position.y + 5f)
            //        {
            //            m_playerRb = this.gameObject.GetComponent<Rigidbody>();
            //            m_playerRb.velocity = this.transform.forward * 100;
            //            m_playerRb.AddForce(this.transform.forward * 100, ForceMode.Impulse);
            //            Debug.Log("Grapple::アドフォース！");
            //        }

            //    }
            //}

            Target();
        }
    }

    private void Target()
    {
        if (LockOnController.IsLock)
        {
            m_currentTarget = m_lockOn.GetTarget;
        }
        else if (!LockOnController.IsLock && m_iot.GetGrapplingTarget)
        {
            m_currentTarget = m_iot.GetGrapplingTarget.gameObject;
        }
        else
        {
            m_currentTarget = null;
        }
    }

    public void Grapple()
    {
        if (!IsHooked)
        {
            IsHooked = true;
            Vector3 lookAtPos = m_currentTarget.transform.position;
            lookAtPos.y = this.transform.position.y;
            this.transform.LookAt(lookAtPos);
            Hook(m_joint, m_currentTarget);
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
        m_stateText.text = "Hook";
        m_targetRb = target.GetComponent<Rigidbody>();
        if (m_targetRb)
        {
            joint.connectedBody = m_targetRb;
            var cj = joint.linearLimit;
            m_playerRb = this.gameObject.GetComponent<Rigidbody>();

            EnableConstraints(this.gameObject);

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
        DisableConstraints(this.gameObject);
        m_stateText.text = "LoseHook";

        joint.connectedBody = null;
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;

        m_latestTarget = m_currentTarget;
        IsHooked = false;
        if (m_anim.GetBool("IsHooking")) m_anim.SetBool("IsHooking", false);

    }

    void DrawLineOnAnimation()
    {
        DrawLine(m_sourcePos.position, m_currentTarget.transform.position);
    }

    void PullTargetOnAnimation()
    {
        PullTarget(m_joint, m_currentTarget);
    }

    void PullTarget(ConfigurableJoint joint, GameObject target)
    {
        m_stateText.text = "PullTarget";

        var cj = joint.linearLimit;

        cj.limit = m_pullLimit;
        joint.linearLimit = cj;
    }

    void DrawLine(Vector3 source, Vector3 destination)
    {
        m_isDrawingLine = true;
        m_lineRend.SetPosition(0, source);
        m_lineRend.SetPosition(1, destination);
    }

    void HideLine()
    {
        DrawLine(Vector3.zero, Vector3.zero);
        m_isDrawingLine = false;
    }

    /// <summary>
    /// 対象のRigidBodyのconstrainsを固定する
    /// </summary>
    void EnableConstraints(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// 対象のRigidBodyのconstrainsを解除する
    /// </summary>
    /// <param name="target"></param>
    void DisableConstraints(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
