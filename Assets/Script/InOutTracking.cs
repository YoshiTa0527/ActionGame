using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class InOutTracking : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] Image icon;
    [SerializeField] GameObject m_grapplingPointParent = null;
    /// <summary>この距離内にあるオブジェクトから一つ選ぶ</summary>
    [SerializeField] float m_distance = 5f;
    GameObject m_nextTarget;
    TargetController m_target;
    public TargetController m_GetTarget { get { return m_target; } }

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
        m_tcs.Clear();
        /*ポイントを全て取得*/
        m_tcs = m_grapplingPointParent.transform.GetComponentsInChildren<TargetController>().ToList();

        m_tcs.ForEach(tcs => Debug.Log("name" + tcs.name));

        var viewport = Camera.main.WorldToViewportPoint(DetectNearlestTarget(m_tcs).transform.position);
        m_target = DetectNearlestTarget(m_tcs);

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
            if (viewport.y > 0.7f)
                icon.rectTransform.anchoredPosition = Rect.NormalizedToPoint(m_canvasRect, viewport);
        }
    }

    /// <summary>
    /// 一番近いターゲットを返す
    /// </summary>
    /// <param name="targetList"></param>
    /// <returns></returns>
    TargetController DetectNearlestTarget(List<TargetController> targetList)
    {
        TargetController nearlestTarget = targetList.OrderBy(t => Vector3.Distance(this.transform.position, t.gameObject.transform.position)).FirstOrDefault();
        Debug.Log($"最も近いポイント{nearlestTarget.gameObject.name}");
        return nearlestTarget;
    }

    /// <summary>
    /// プレイヤーから一定距離内のオブジェクトを返す
    /// </summary>
    /// <param name="targetList"></param>
    /// <returns></return>
    TargetController[] DetectNearTargets(List<TargetController> targetList)
    {
        TargetController[] nearlestTargets = targetList.Where(t => Vector3.Distance(this.transform.position, t.transform.position) < m_distance).ToArray();
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
    List<TargetController> GetTarget(List<TargetController> targets)
    {
        /*カメラより前方のものを探す*/

        TargetController[] nearTargets = DetectNearTargets(targets);

        return nearTargets.ToList().Where(target =>
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.transform.position);
            return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        }).ToList();

        //protected List<GameObject> FilterTargetObject(List<GameObject> hits)
        //{
        //    return hits
        //        .Where(h => {
        //            Vector3 screenPoint = Camera.main.WorldToViewportPoint(h.transform.position);
        //            return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        //        })
        //        .Where(h => h.tag == "Enemy")
        //        .ToList();
        //}
    }
}