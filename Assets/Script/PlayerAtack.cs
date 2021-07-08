using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAtack : MonoBehaviour
{
    SimpleAnimation m_simpleAnimation;
    private void Start()
    {
        m_simpleAnimation = GetComponent<SimpleAnimation>();
    }
    private void Update()
    {
        if (Input.GetButtonDown("Atack"))
        {
            m_simpleAnimation.CrossFade("Atack", 0.1f);
            Debug.Log("atack");
        }
    }
}
