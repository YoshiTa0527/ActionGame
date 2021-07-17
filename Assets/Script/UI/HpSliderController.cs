using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///　このコンポーネントがアタッチされているオブジェクトがロックオンされているなら表示し続け、そうでないならダメージを受けた一定時間後消える
/// </summary>
public class HpSliderController : MonoBehaviour
{
    [SerializeField] float m_inactiveTime = 1f;
    float m_timer;
    private void Start()
    {
        this.gameObject.SetActive(false);
        m_timer = 0f;
    }

    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer > m_inactiveTime)
        {
         
            this.gameObject.SetActive(false);
        }
    }
}
