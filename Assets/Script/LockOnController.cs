using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Cinemachine;

/// <summary>
/// 敵をロックオンする。3/3ロック送り機能を追加することとm_targetがnullの時の対処
/// </summary>
public class LockOnController : MonoBehaviour
{
    [SerializeField] Image m_lockOnMarkerImage;
    [SerializeField] float m_lockOnRange = 5f;
    /// <summary>現在ターゲット可能な敵のリスト</summary>
    List<EnemyTargetController> m_targets = new List<EnemyTargetController>();
    List<EnemyTargetController> m_orderedTargets = new List<EnemyTargetController>();
    EnemyTargetController m_target;
    EnemyTargetController m_currentTarget;
    /// <summary>ロックオン時に使うVcam</summary>
    CinemachineVirtualCamera m_targetCamera;
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
    float m_timer = 0;
    CinemachineTargetGroup m_targetGroup;

    private void Start()
    {
        m_enemyParent = GameObject.FindGameObjectWithTag("EnemyParent");
        IsLock = false;
        m_targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        m_player = GameObject.FindGameObjectWithTag("Player");
        /*ターゲットカメラに関する処理*/
        m_targetCamera = GameObject.FindGameObjectWithTag("TargetCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        m_priority = m_targetCamera.Priority;
        m_targetCamera.Priority = -1;

    }

    private void Update()
    {
        if (!m_player) return;
        m_targets.Clear();

        // 現在のターゲットから画面から消えた、または射程距離外に外れたら、ターゲットを消す
        if (m_target && (!m_target.IsHookable || Vector3.Distance(m_target.transform.position, m_player.transform.position) > m_lockOnRange))
        {
            UnLockEnemy();
            m_target = null;

            Debug.Log("target lost.");
        }

        /*とりあえず敵を全て取得する*/
        EnemyTargetController[] targets = m_enemyParent.transform.GetComponentsInChildren<EnemyTargetController>();

        /*取得した敵を振り分ける。カメラに写っており、ロックオン可能な距離にいる敵をリストに入れる*/
        foreach (var t in targets)
        {
            if (t.IsHookable && m_lockOnRange > Vector3.Distance(m_player.transform.position, t.transform.position))
            {
                m_targets.Add(t);
                Debug.Log($"ロックオン可能な敵の数{m_targets.Count.ToString()}");
            }
        }

        /*もしロックオン状態だったら、ターゲットの位置にロックオンカーソルを表示し続ける*/
        if (IsLock && m_lockOnMarkerImage && m_target)
        {
            m_lockOnMarkerImage.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.transform.position);

        }
        else { UnLockEnemy(); }

        if (IsLock && m_orderedTargets.Count >= 0)
        {
            /*複数の敵がロックオン可能な場合十字キー左右で敵の選択*/
            float horizon = Input.GetAxis("D-Pad H");
            if (horizon > 0)
            {
                /*感度が良すぎるので少し待ってからターゲットを切り替えたい*/
                m_timer += Time.deltaTime;
                if (m_timer > 0.1f)
                {
                    m_timer = 0;

                    Debug.Log($"Input::十字キー左右が押された。{m_targetIndex.ToString()}");
                    m_targetIndex = (m_targetIndex + 1) % m_orderedTargets.Count;
                }
                m_target = m_orderedTargets[m_targetIndex];
            }
        }

        /*スティック押し込みで敵をロックオンする*/
        if (Input.GetButtonDown("PadStickPush") && m_targets.Count > 0)
        {
            m_targetIndex = 0;
            Debug.Log("スティック押し込み");
            if (!IsLock)
            {
                DetectNearestTarget();
                m_target = m_orderedTargets[m_targetIndex];
                LockOnEnemy();
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
    void LockOnEnemy()
    {
        Debug.Log("Lock on Enemy");
        IsLock = true;
        if (m_target) m_player.transform.LookAt(m_target.transform);
        if (m_targetGroup) m_targetGroup.AddMember(m_target.transform, m_weight, m_radius);
        if (m_targetCamera) m_targetCamera.Priority = m_priority;
    }
    /// <summary>
    /// 敵のロックオンを外す
    /// </summary>
    void UnLockEnemy()
    {
        Debug.Log("UnLock Enemy");
        IsLock = false;
        HideMarker();
        if (m_targetGroup.m_Targets.Length > 1) m_targetGroup.RemoveMember(m_target.transform);
        if (m_targetCamera) m_targetCamera.Priority = -1;

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
    /// 敵を近い順に振り分けたリストを返す
    /// </summary>
    /// <returns></returns>
    void DetectNearestTarget()
    {
        m_orderedTargets = m_targets.OrderBy(t => Vector3.Distance(m_player.transform.position, t.transform.position)).ToList();

    }

}
