using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint))]
public class GrapplingManager : MonoBehaviour
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
        if (m_currentTarget) m_targetText.text = $"Target:{m_currentTarget.name}";
        else m_targetText.text = $"Target:null";
        /*ロックオン中ならターゲットにフックをつける*/
        if (LockOnController.IsLock)
        {
            m_currentTarget = m_lockOn.GetTarget;
            if (Input.GetButtonDown("Fire1") && Vector3.Distance(this.transform.position, m_currentTarget.transform.position) > 5f)
            {
                if (!IsHooked)
                {
                    IsHooked = true;
                    Hook(m_joint, m_currentTarget);
                }
                //else IsHooked = false;
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
                // else IsHooked = false;
            }
        }
        else
        {
            m_currentTarget = null;
            IsHooked = false;
        }

        if (IsHooked)
        {
            m_timer += Time.deltaTime;
            if (m_timer > m_pullTime)
            {
                m_timer = 0;
                PullTarget(m_joint, m_currentTarget);
            }

            DrawLine(m_sourcePos.position, m_currentTarget.transform.position);

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
                        && this.transform.position.y > m_currentTarget.transform.position.y)
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
                if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget    　// && CalcValue(yAbs, 1f)
                    && this.transform.position.y > m_currentTarget.transform.position.y)
                {
                    Debug.Log("高さほぼ一緒");
                    m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                    m_playerRb.velocity = Vector3.zero;
                    m_playerRb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                    LoseHook(m_joint);
                }

                //float y = m_currentTarget.transform.position.y - this.transform.position.y;
                //if (y > 0)//ポイントがプレイヤーより上にある場合
                //{
                //    if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToDecelerateTarget)
                //    {
                //        ターゲットの上方へ飛ぶ
                //        var targetUp = (m_currentTarget.transform.position - this.transform.position) + Vector3.up;
                //        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                //        m_playerRb.AddForce(targetUp * 5f, ForceMode.Impulse);
                //        LoseHook(m_joint);
                //    }
                //}
                //else if (y < 0)//ポイントがプレイヤーより下にある場合
                //{

                //}
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

        IsHooked = false;
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
        m_lineRend.SetPosition(0, source);
        m_lineRend.SetPosition(1, destination);
    }

    void HideLine()
    {
        DrawLine(Vector3.zero, Vector3.zero);
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
