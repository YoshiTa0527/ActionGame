using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DpadController : MonoBehaviour
{
    public static bool m_dpadRight;
    public static bool m_dpadLeft;
    private static float m_currentDpadValue;

    private void Start()
    {
        Debug.Log("DpadClassActive");
        m_currentDpadValue = 0f;
    }
    private void Update()
    {
        //押されたときに、数値が前と一緒だったら押せなくしたい
        float dpadValue = Input.GetAxisRaw("D-Pad H");

        if (dpadValue != 0 && m_currentDpadValue == 0f)
        {
            if (dpadValue > 0)
            {
                m_dpadRight = true;
            }
            else if (dpadValue < 0)
            {
                m_dpadLeft = true;
            }
        }
        else
        {
            m_dpadRight = false;
            m_dpadLeft = false;
        }

        m_currentDpadValue = dpadValue;

        if (m_dpadLeft)
        {
            Debug.Log("test::左が押された");
            // dpadLeft = false;
        }
        else if (m_dpadRight)
        {
            Debug.Log("test::右が押された");
            //dpadRight = false;
        }
    }
}
