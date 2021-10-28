using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTest : MonoBehaviour
{
    ConfigurableJoint m_joint;
    LineRenderer m_linRend;
    [SerializeField] Rigidbody m_targetRb;
    [SerializeField] float m_turnSpeed = 10;
    bool m_isHooking;
    private void Start()
    {
        m_joint = GetComponent<ConfigurableJoint>();
        m_linRend = GetComponent<LineRenderer>();
    }

    private void Update()
    {

        if (m_isHooking) LineRend();
    }

    public void Hook()
    {
        m_joint.connectedBody = m_targetRb;
        var cj = m_joint.linearLimit;
       // cj.limit = Vector3.Distance(this.transform.position, m_targetRb.transform.position);
       // m_joint.linearLimit = cj;
        m_isHooking = true;
        LineRend();
    }

    private void LineRend()
    {
        m_linRend.SetPosition(0, this.transform.position);
        m_linRend.SetPosition(1, m_targetRb.gameObject.transform.position);
    }
}
