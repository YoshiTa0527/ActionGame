using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    static bool m_isSlow;
    float m_timer;
    float m_defaultTimeScale;
    public static float m_hitStopDuration { get; set; }
    private void Start()
    {
        m_defaultTimeScale = Time.timeScale;
    }

    private void Update()
    {
        if (!m_isSlow) return;
        EndHitStopOnUpdate();
    }
    public static void HitStop(float scale)
    {
        Debug.Log("hitstopStart");
        m_isSlow = true;
        Time.timeScale = scale;
    }

    void EndHitStopOnUpdate()
    {
        m_timer += Time.unscaledDeltaTime;
        if (m_timer >= m_hitStopDuration)
        {
            m_isSlow = false;
            m_timer = 0;
            Time.timeScale = m_defaultTimeScale;
            Debug.Log("hitstopEnd");
        }
    }
}
