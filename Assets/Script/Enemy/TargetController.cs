using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エネミーにアタッチするコンポーネント
///　ターゲットできるエネミーを判定する
///　OnBecameVislble/OnBecameInvisible は「シーンビューのカメラ」も影響することに注意すること。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TargetController : MonoBehaviour
{
    bool m_isTarget = false;
    bool m_isTargetable = false;
    [SerializeField] public UnityEngine.UI.Text m_orderText = null;

    /// <summary>
    /// オブジェクトがロックオンされているかどうか返す
    /// </summary>

    public bool IsTargeted { get => m_isTarget; set { m_isTarget = value; } }

    /// <summary>
    /// オブジェクトが画面内にあるかどうかを返す
    /// </summary>
    public bool IsVisible
    {
        get { return m_isTargetable; }
    }

    private void OnBecameVisible()
    {
        m_isTargetable = true;
        Debug.Log($"{this.gameObject.name}::OnBecameVisible()");
    }

    private void OnBecameInvisible()
    {
        m_isTargetable = false;
        m_isTarget = false;
        Debug.Log($"{this.gameObject.name}::OnBecameInvisible()");
    }

}
