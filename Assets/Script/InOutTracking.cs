using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class InOutTracking : MonoBehaviour
{

    [SerializeField] Transform testTarget;
    [SerializeField] Image icon;
    [SerializeField] GameObject m_grapplingPointParent = null;
    /// <summary>この距離内にあるオブジェクトから一つ選ぶ</summary>
    [SerializeField] float m_maxDistance = 10f;
    GameObject m_nextTarget;
    TargetController m_target;
    public TargetController GetGrapplingTarget { get { return m_target; } }

    Rect m_canvasRect;
    List<TargetController> m_tcs = new List<TargetController>();


    void Start()
    {
        // UIがはみ出ないようにする
        m_canvasRect = ((RectTransform)icon.canvas.transform).rect;
        m_canvasRect.Set(
            m_canvasRect.x + icon.rectTransform.rect.width * 0.5f,
            m_canvasRect.y + icon.rectTransform.rect.height * 0.5f,
            m_canvasRect.width - icon.rectTransform.rect.width,
            m_canvasRect.height - icon.rectTransform.rect.height
        );
    }
    /*ターゲットの取得方法
     * 一定の距離以内に制限する→一定時間沖にスフィアキャストをして取得する？→フィールド上の
     * 取得したもののうち、画面中央に最も近いものをターゲットにする
     */
    void Update()
    {
        if (!m_grapplingPointParent) return;
        m_tcs.Clear();
        /*ポイントを全て取得*/
        m_tcs = m_grapplingPointParent.transform.GetComponentsInChildren<TargetController>().ToList();
        /*プレイヤーがグラップル中の時はターゲットを変えない*/
        if (!GrapplingManager.IsHooked) m_target = GetTarget(m_tcs);

        if (m_target)
        {
            Debug.Log($"target:{m_target.gameObject.name}");
            icon.enabled = true;
            var viewport = Camera.main.WorldToViewportPoint(m_target.transform.position);
            Debug.Log($"viewport:{viewport}");
            if (m_target.IsHookable)
            {
                icon.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.transform.position);
            }
            else if (!m_target.IsHookable)
            {
                // 画面内で対象を追跡
                viewport.x = Mathf.Clamp01(viewport.x);
                viewport.y = Mathf.Clamp01(viewport.y);
                Debug.Log($"x::{viewport.x}y;;{viewport.x}");
                icon.rectTransform.anchoredPosition = Rect.NormalizedToPoint(m_canvasRect, viewport);
            }
        }
        else icon.enabled = false;
    }

    /// <summary>
    /// プレイヤーから一定距離内のオブジェクトを配列にして返す
    /// </summary>
    /// <param name="targetList"></param>
    /// <returns></return>
    TargetController[] DetectNearTargets(List<TargetController> targetList)
    {
        TargetController[] nearlestTargets = targetList.Where(t =>
                                                         Vector3.Distance(this.transform.position, t.transform.position) < m_maxDistance &&
                                                         t.transform.position.y > this.transform.position.y)
                                                          .ToArray();

        return nearlestTargets;
    }

    /// <summary>
    /// 条件に当てはまるターゲットを探す
    /// 
    /// カメラより前方。
    /// 画面の中心に近いもの。
    /// カメラが見ているベクトルと、全てのオブジェクトとカメラのトランスフォームのベクトルを取って外積をとる
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    TargetController GetTarget(List<TargetController> targets)
    {
        var nearTergets = DetectNearTargets(targets);

        /*一定距離内に何もなかったら何もしない*/
        if (nearTergets.Count() == 0)
        {
            m_target = null;
            return null;
        }

        /*一定距離内のターゲットの中で、画面に映っており、プレイヤーより上にあるもの*/
        var nearAndVisibleTarget = nearTergets.Where(t => t.IsHookable == true);

        /*一定距離内のターゲットの中で、画面に映っているものがあるときは、画面の中央に近いものをターゲットとする*/
        if (nearAndVisibleTarget.Count() > 0)
        {
            float minTargetDistance = float.MaxValue;
            TargetController target = null;
            foreach (var t in nearAndVisibleTarget)
            {
                Vector3 targetScreenPoint = Camera.main.WorldToViewportPoint(t.transform.position);
                float targetDistance = Vector2.Distance(
                    new Vector2(0.5f, 0.5f),//画面中央
                    new Vector2(targetScreenPoint.x, targetScreenPoint.y)
                );
                Debug.Log($"ターゲット:{t}とのスクリーン上の距離:" + targetDistance);
                if (targetDistance < minTargetDistance)
                {
                    minTargetDistance = targetDistance;
                    target = t;
                }
            }
            return target;
        }
        else /*画面内に写っているものが無ければ最も近いものがターゲット*/
        {
            return DetectNearTargets(targets).FirstOrDefault();
        }


    }
}