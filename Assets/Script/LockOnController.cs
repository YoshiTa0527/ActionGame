using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


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
    public GameObject GetTarget { get { return m_target.gameObject; } }
    GameObject m_enemyParent;
    public static bool IsLock { get; set; }
    GameObject player;
    int m_targetIndex = 0;
    float m_timer = 0;

    private void Start()
    {
        m_enemyParent = GameObject.FindGameObjectWithTag("EnemyParent");
        IsLock = false;
    }

    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;
        m_targets.Clear();

        // 現在のターゲットから画面から消えた、または射程距離外に外れたら、ターゲットを消す
        if (m_target && (!m_target.IsHookable || Vector3.Distance(m_target.transform.position, player.transform.position) > m_lockOnRange))
        {
            m_target = null;
            UnLockEnemy();
            Debug.Log("target lost.");
        }



        /*とりあえず敵を全て取得する*/
        EnemyTargetController[] targets = m_enemyParent.transform.GetComponentsInChildren<EnemyTargetController>();

        /*取得した敵を振り分ける。カメラに写っており、ロックオン可能な距離にいる敵をリストに入れる*/
        foreach (var t in targets)
        {
            if (t.IsHookable && m_lockOnRange > Vector3.Distance(player.transform.position, t.transform.position))
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
            if (horizon > 0 || horizon < 0)
            {
                /*感度が良すぎるので少し待ってからターゲットを切り替えたい*/
                m_timer += Time.deltaTime;
                if (m_timer > 0.1f)
                {
                    m_timer = 0;
                    m_targetIndex += (int)horizon;
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
    }
    /// <summary>
    /// 敵のロックオンを外す
    /// </summary>
    void UnLockEnemy()
    {
        Debug.Log("UnLock Enemy");
        IsLock = false;
        HideMarker();
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
        m_orderedTargets = m_targets.OrderBy(t => Vector3.Distance(player.transform.position, t.transform.position)).ToList();

    }

}
