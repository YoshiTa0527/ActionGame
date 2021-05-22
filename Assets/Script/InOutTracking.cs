using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InOutTracking : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] Image icon;
    [SerializeField] GameObject m_grapplingPointParent = null;
    GameObject m_nextTarget;
    GameObject m_target;
    public GameObject m_GetTarget { get { return m_target; } }

    Rect canvasRect;
    TargetController[] m_tcs;


    void Start()
    {
        // UIがはみ出ないようにする
        canvasRect = ((RectTransform)icon.canvas.transform).rect;
        canvasRect.Set(
            canvasRect.x + icon.rectTransform.rect.width * 0.5f,
            canvasRect.y + icon.rectTransform.rect.height * 0.5f,
            canvasRect.width - icon.rectTransform.rect.width,
            canvasRect.height - icon.rectTransform.rect.height
        );
    }
    /*グラップルポイントを全てリストに入れる
     一番近いポイントが、画面内に写っている場合、グラップルポイント用のカーソルを表示する
    　一番近いポイントにプレイヤーがグラップルすると、二番目に近いポイントがターゲットになる*/
    void Update()
    {
        m_tcs = m_grapplingPointParent.transform.GetComponentsInChildren<TargetController>();

        var viewport = Camera.main.WorldToViewportPoint(DetectNearlestTarget(m_tcs).transform.position);

        if (DetectNearlestTarget(m_tcs).IsHookable)
        {
            icon.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.transform.position);
        }
        else if (!DetectNearlestTarget(m_tcs).IsHookable)
        {
            // 画面内で対象を追跡
            viewport.x = Mathf.Clamp01(viewport.x);
            viewport.y = Mathf.Clamp01(viewport.y);
            icon.rectTransform.anchoredPosition = Rect.NormalizedToPoint(canvasRect, viewport);
        }
    }

    TargetController DetectNearlestTarget(TargetController[] targetArray)
    {
        TargetController nearlestTarget = targetArray.OrderBy(t => Vector3.Distance(this.transform.position, t.gameObject.transform.position)).First();
        return nearlestTarget;
    }
}