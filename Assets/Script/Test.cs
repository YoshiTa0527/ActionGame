using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook m_fcum;
    Transform m_followTemp, m_lookAtTemp;
    bool m_isPushedButton = false;

    private void Start()
    {
        m_followTemp = m_fcum.Follow;
        m_lookAtTemp = m_fcum.LookAt;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("PadStickPush"))
        {
            Debug.Log("Test::PushedButton");
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (!m_isPushedButton)
            {
                Debug.Log("Test");
                m_isPushedButton = true;
                m_fcum.Follow = null;
                m_fcum.LookAt = null;
                m_fcum.transform.position = Vector3.zero;
            }
            else
            {
                m_isPushedButton = false;
                m_fcum.Follow = m_followTemp;
                m_fcum.LookAt = m_lookAtTemp;
            }
        }
    }
}
