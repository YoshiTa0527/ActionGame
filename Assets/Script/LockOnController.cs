using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Cinemachine;

/// <summary>
/// 敵をロックオンする。
/// </summary>
public class LockOnController : MonoBehaviour
{
    [SerializeField] Image m_lockOnMarkerImage = null;
    [SerializeField] float m_lockOnRange = 5f;
    /// <summary>現在ターゲット可能な敵のリスト</summary>
    List<TargetController> m_targets = new List<TargetController>();
    /// <summary>近い順に並び変えた敵のリスト</summary>
    List<TargetController> m_orderedTargets = new List<TargetController>();
    TargetController m_target;
    /// <summary>ロックオン時に使うVcam</summary>
    CinemachineVirtualCamera m_targetCamera;
    CinemachineFreeLook m_fcum;
    Transform m_followTemp, m_lookAtTemp;
    /// <summary>vcamのプライオリティの初期値を覚えておく</summary>
    int m_priority;
    /// <summary>ターゲットグループに追加するときのradius</summary>
    [SerializeField] float m_radius = 5f;
    [SerializeField] float m_weight = 5f;

    public GameObject GetTarget { get { return m_target.gameObject; } }
    GameObject m_enemyParent;
    public static bool IsLock { get; set; }
    GameObject m_player;
    int m_targetIndex = 0;
    CinemachineTargetGroup m_targetGroup;
    [SerializeField] bool m_isDebugmode = false;

    private void Start()
    {
        m_enemyParent = GameObject.FindGameObjectWithTag("EnemyParent");
        m_player = GameObject.FindGameObjectWithTag("Player");
        IsLock = false;
        /*ターゲットカメラに関する処理*/
        m_targetCamera = GameObject.FindGameObjectWithTag("TargetCamera")?.gameObject.GetComponent<CinemachineVirtualCamera>();
        m_targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        if (m_targetCamera)
        {
            m_priority = m_targetCamera.Priority;
            m_targetCamera.Priority = -1;
            m_targetCamera.Follow = null;
        }
        m_fcum = GameObject.FindGameObjectWithTag("FreeLookCamera1").gameObject.GetComponent<CinemachineFreeLook>();
        if (m_fcum)
        {
            Debug.Log("through");
            m_followTemp = m_fcum.Follow;
            m_lookAtTemp = m_fcum.LookAt;
        }
    }

    private void Update()
    {
        if (!m_player) return;
        m_targets.Clear();
        if (m_targetGroup && m_targetGroup.m_Targets.Length > 2) m_targetGroup.RemoveMember(m_targetGroup.m_Targets[1].target.transform);
        //現在のターゲットから画面から消えた、または射程距離外に外れたら、ターゲットを消す
        if (m_target)
        {
            Debug.Log("LockOnContrtoller::targetName=" + m_target.gameObject.name);
            if (!m_target.IsHookable || Vector3.Distance(m_target.transform.position, m_player.transform.position) > m_lockOnRange)
            {
                UnLockEnemy();
                m_target = null;
                Debug.Log("target lost.");
            }
        }

        ///*とりあえずターゲットできるものを全て取得する*/
        TargetController[] targets = m_enemyParent.transform.GetComponentsInChildren<TargetController>();
        /*取得した敵を振り分ける。カメラに写っており、ロックオン可能な距離にいる敵をリストに入れる*/
        foreach (var t in targets)
        {
            if (t.IsHookable && m_lockOnRange > Vector3.Distance(m_player.transform.position, t.transform.position))
            {
                m_targets.Add(t);
                Debug.Log($"ロックオン可能な敵の数{m_targets.Count}");
            }
        }

        if (m_isDebugmode)
        {
            DetectNearestTarget();
        }

        if (IsLock)
        {
            if (m_fcum && m_targetCamera) m_fcum.transform.position = m_targetCamera.transform.position;
            if (m_lockOnMarkerImage && m_target)
                m_lockOnMarkerImage.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.transform.position);

            if (m_orderedTargets.Count >= 0)
            {
                /*複数の敵がロックオン可能な場合十字キー左右で敵の選択*/
                if (DpadController.m_dpadRight)
                {
                    m_targetIndex = (m_targetIndex + 1) % m_orderedTargets.Count;
                    Debug.Log("LockOnController::IsHookable" + m_orderedTargets[m_targetIndex].IsHookable);
                    if (m_orderedTargets[m_targetIndex].IsHookable) LockOnEnemy(m_orderedTargets[m_targetIndex]);
                }
            }
        }
        else
        {
            UnLockEnemy();//here
            if (m_targetCamera) m_targetCamera.transform.position = m_fcum.transform.position;
        }

        /*スティック押し込みで敵をロックオンする*/
        if (Input.GetButtonDown("PadStickPush") && m_targets.Count > 0)
        {
            m_targetIndex = 0;
            Debug.Log("スティック押し込み");
            if (!IsLock)
            {
                DetectNearestTarget();
                LockOnEnemy(m_orderedTargets[m_targetIndex]);
            }
            else
            {
                UnLockEnemy();
            }
        }
    }
    /// <summary>
    /// 敵をロックオンする
    /// </summary>
    public void LockOnEnemy(TargetController target)
    {
        Debug.Log("Lock on Enemy");
        if (m_target == target)
            return;
        m_target = target;
        if (m_target)
        {
            Vector3 lookAtTarget = target.transform.position;
            lookAtTarget.y = m_player.transform.position.y;
            m_player.transform.LookAt(lookAtTarget);
        }
        if (m_targetGroup) m_targetGroup.AddMember(m_target.transform, m_weight, m_radius);
        if (!IsLock)
        {
            if (m_targetCamera)
            {
                m_targetCamera.Follow = m_followTemp;
                m_targetCamera.Priority = m_priority;
            }
            /*フリールックのfollowとlookAtを空にしないとtransformを操作できない*/
            if (m_fcum)
            {
                m_fcum.Follow = null;
                m_fcum.LookAt = null;
            }
        }
        IsLock = true;
    }
    /// <summary>
    /// 敵のロックオンを外す
    /// </summary>
    public void UnLockEnemy()
    {
        Debug.Log("UnLock Enemy");
        IsLock = false;
        HideMarker();
        if (m_targetGroup && m_targetGroup.m_Targets.Length > 1) m_targetGroup.RemoveMember(m_target.transform);
        if (m_targetCamera)
        {
            m_targetCamera.Follow = null;
            m_targetCamera.Priority = -1;
        }
        /*フリールックのtransformにグループカメラのそれを代入し、フリールックの設定を元に戻す*/
        if (m_fcum)
        {
            m_fcum.Follow = m_followTemp;
            m_fcum.LookAt = m_lookAtTemp;
        }
        m_target = null;

    }
    /// <summary>
    /// 指定された位置にマーカーを表示する
    /// </summary>
    /// <param name="position"></param>
    public void AppearMarker(Vector3 position)
    {
        if (m_lockOnMarkerImage)
        {
            m_lockOnMarkerImage.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
        }
    }

    /// <summary>
    /// ロックオンマーカーを隠す
    /// </summary>
    public void HideMarker()
    {
        if (m_lockOnMarkerImage)
        {
            m_lockOnMarkerImage.rectTransform.position = -1 * m_lockOnMarkerImage.rectTransform.rect.size;
        }
    }

    /// <summary>
    /// リストの中の敵を並び替える
    /// </summary>
    /// <returns></returns>
    void DetectNearestTarget()
    {
        Debug.Log("リストの中の敵を並び替える");

        if (m_isDebugmode)
            m_orderedTargets.ForEach(t => t.gameObject.transform.Find("EnemyCanvas").
                                            gameObject.transform.Find("Text").GetComponent<Text>().text = m_orderedTargets.IndexOf(t).ToString());
        m_orderedTargets = m_targets.OrderBy(t => Vector3.Distance(m_player.transform.position, t.transform.position)).ToList();
    }

}
