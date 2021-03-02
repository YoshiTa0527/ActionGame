using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


/// <summary>
/// 敵をロックオンする
/// </summary>
public class LockOnController : MonoBehaviour
{
    [SerializeField] Image m_lockOnMarkerImage;
    [SerializeField] float m_lockOnRange = 5f;
    /// <summary>現在ターゲット可能な敵のリスト</summary>
    List<EnemyTargetController> m_targets = new List<EnemyTargetController>();
    EnemyTargetController m_target;
    public GameObject GetTarget { get { return m_target.gameObject; } }
    GameObject m_enemyParent;
    public static bool IsLock { get; set; }
    GameObject player;


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

        /*スティック押し込みで敵をロックオンする*/

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

        /*一番近い敵をターゲットする*/
        if (!m_target && m_targets.Count > 0)
        {
            m_target = m_targets.OrderBy(t => Vector3.Distance(player.transform.position, t.transform.position)).First();
        }

        /*もしロックオン状態だったら、ターゲットの位置にロックオンカーソルを表示し続ける*/
        if (IsLock && m_lockOnMarkerImage && m_target)
        {
            m_lockOnMarkerImage.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.transform.position);
        }
        else { UnLockEnemy(); }

        if (Input.GetButtonDown("PadStickPush"))
        {
            Debug.Log("スティック押し込み");
            if (!IsLock)
            {

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

}
