using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharactorController : MonoBehaviour
{
    [SerializeField] float m_turnSpeed = 10;
    [SerializeField] JointTest m_joint;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float angleX = Input.GetAxisRaw("PadStickRX");
        float angleZ = Input.GetAxisRaw("PadStickRY");

        Vector3 dir = Vector3.forward * angleX + Vector3.right * angleZ;
        Quaternion targetRoatation = Quaternion.LookRotation(dir);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRoatation, m_turnSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Fire1")) m_joint.Hook();
    }
}
