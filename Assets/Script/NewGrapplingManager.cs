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
    [SerializeField] float m_disToTargetForEnemy = 2f;
    [SerializeField] float m_disToTargetForEnemyInTheAir = 2f;
    [SerializeField] float m_disToTargetForTarget = 2f;
    /// <summary>ターゲットを減速させるときにかける数</summary>
    [SerializeField] float m_decelerateSpeed = 0.85f;
    /// <summary>直前のターゲット</summary>
    GameObject m_latestTarget;
    [SerializeField] bool isEnable;
    bool m_haveTarget;
    float m_distanceToTarget;
    public bool GetHaveTarget { get => m_haveTarget; }
    bool m_isDrawingLine;

    [SerializeField] InOutTracking m_iot;

    GameObject m_currentTarget = null;
    PlayerController m_player;
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
        m_player = this.gameObject.GetComponent<PlayerController>();
        m_anim = GetComponent<Animator>();
        if (!m_sourcePos) m_sourcePos = this.transform;
        HideLine();
    }

    private void Update()
    {
        if (!isEnable) return;
        if (m_currentTarget && Vector3.Distance(this.transform.position, m_currentTarget.transform.position) >= m_disToTargetForEnemyInTheAir)
        {
            if (m_targetText) m_targetText.text = $"Target:{m_currentTarget.name}::{m_haveTarget}";
            m_haveTarget = true;
        }
        else
        {
            if (m_targetText) m_targetText.text = $"Target:{m_currentTarget}::{m_haveTarget}";
            m_haveTarget = false;
        }

        if (IsHooked)
        {
            if (m_currentTarget.tag == "Enemy")
            {
                EnemyController enemyController = m_currentTarget.GetComponent<EnemyController>();
                if (enemyController.EnemyIsGround)
                {
                    EnableConstraints(this.gameObject);
                    m_targetRb = m_currentTarget.GetComponent<Rigidbody>();
                    m_targetRb.velocity = Vector3.zero;

                    if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToTargetForEnemy)
                    {
                        Debug.Log("敵に近づいた");
                        LoseHook(m_joint);
                    }
                }
                else
                {
                    EnableConstraints(m_currentTarget);
                    DisableConstraints(this.gameObject);
                    bool isHigher = this.transform.position.y >= m_currentTarget.transform.position.y;
                    float dis = Vector3.Distance(this.transform.position, m_currentTarget.transform.position);
                    if (dis <= m_disToTargetForEnemyInTheAir && isHigher)
                    {
                        Debug.Log("高さほぼ一緒");
                        m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                        m_playerRb.velocity = Vector3.zero;
                        LoseHook(m_joint);
                    }
                }
            }
            else if (m_currentTarget.tag == "GrapplePoint")
            {
                DisableConstraints(this.gameObject);

                if (Vector3.Distance(this.transform.position, m_currentTarget.transform.position) <= m_disToTargetForTarget)
                {
                    m_playerRb = this.gameObject.GetComponent<Rigidbody>();
                    m_playerRb.velocity *= m_decelerateSpeed;
                    LoseHook(m_joint);
                }
            }

            if (m_isDrawingLine) DrawLine(m_sourcePos.position, m_currentTarget.transform.position);
        }
        else
        {
            Target();
        }
    }

    private void Target()
    {
        if (LockOnController.IsLock)
        {
            m_currentTarget = m_lockOn?.GetTarget;
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
        else if (IsHooked)
        {
            IsHooked = false;
            LoseHook(m_joint);
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
        return -1 * threshold < x && x < threshold;
    }

    void Hook(ConfigurableJoint joint, GameObject target)
    {
        m_stateText.text = "Hook";
        m_targetRb = target.GetComponent<Rigidbody>();
        m_distanceToTarget = Vector3.Distance(this.transform.position, m_currentTarget.transform.position);
        if (m_targetRb)
        {
            if (m_targetRb.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemyController = m_targetRb.gameObject.GetComponent<EnemyController>();
                enemyController.EnemyStateProp = EnemyController.EnemyState.Hooked;
            }
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

    /// <summary>
    /// 徐々にlimitを短くする
    /// </summary>
    void TowingTarget(ConfigurableJoint joint)
    {
        m_stateText.text = "PullTarget";

        var cj = joint.linearLimit;

        while (true)
        {
            cj.limit *= 0.99f;
            joint.linearLimit = cj;

            if (joint.linearLimit.limit <= 1) break;
        }

        Debug.Log("end");
    }

    IEnumerator TowingTargetEnum()
    {
        var cj = m_joint.linearLimit;
        while (true)
        {
            if (m_joint.linearLimit.limit <= 1 || !IsHooked) break;

            cj.limit *= 0.9f;
            m_joint.linearLimit = cj;

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void TowingTargetOnAnimation()
    {
        StartCoroutine(TowingTargetEnum());
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
        rb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
    }
}
