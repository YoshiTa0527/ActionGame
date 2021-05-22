using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


public class Test : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Camera targetCamera;
    [SerializeField] Image icon;

    Rect rect = new Rect(0, 0, 1, 1);

    Rect canvasRect;

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

    void Update()
    {
        var viewport = targetCamera.WorldToViewportPoint(target.position);
        if (rect.Contains(viewport))
        {
            icon.enabled = false;
        }
        else
        {
            icon.enabled = true;

            // 画面内で対象を追跡
            viewport.x = Mathf.Clamp01(viewport.x);
            viewport.y = Mathf.Clamp01(viewport.y);
            icon.rectTransform.anchoredPosition = Rect.NormalizedToPoint(canvasRect, viewport);
        }
    }
}
