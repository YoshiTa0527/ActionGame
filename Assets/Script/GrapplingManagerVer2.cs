using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingManagerVer2 : MonoBehaviour
{

    /// <summary>ボタンを押すと、jointの制限距離を短くする </summary>
    [SerializeField] Transform m_pullPoint;

    SpringJoint m_joint;
    LockOnController m_lockOn;
    LineRenderer m_lineRend;
    private bool m_isGrappling = false;

    private void Start()
    {
        m_lineRend = GetComponent<LineRenderer>();
        m_joint = GetComponent<SpringJoint>();
        m_lockOn = FindObjectOfType<LockOnController>();
    }
    private void Update()
    {
        /*ロックオン中ならターゲットにフックをつける*/
        if (LockOnController.IsLock)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!m_isGrappling)
                {
                    m_isGrappling = true;
                }
                else m_isGrappling = false;
            }

        }
        else
        {
            m_isGrappling = false;
        }

        if (m_isGrappling)
        {
            if (Input.GetButtonDown("Fire2"))
            {
               
                Debug.Log("pushed Fire2");
            }
            DrawLine(this.transform.position, m_lockOn.GetTarget.transform.position);

        }
        else
        {
           
            HideLine();
        }


    }

    void Hook()
    {

    }

    void LoseHook()
    { }


    void PullTarget()
    {

    }

    void DrawLine(Vector3 source, Vector3 destination)
    {
        m_lineRend.SetPosition(0, source);
        m_lineRend.SetPosition(1, destination);
    }

    void HideLine()
    {
        DrawLine(Vector3.zero, Vector3.zero);
    }

}
