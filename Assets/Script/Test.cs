﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Test : MonoBehaviour
{
    static bool dpadRight;
    static bool dpadLeft;
    static float m_currentDpadValue;

    private void Start()
    {
        Debug.Log("TestClassActive");
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
                dpadRight = true;
            }
            else if (dpadValue < 0)
            {
                dpadLeft = true;
            }
        }
        else
        {
            dpadRight = false;
            dpadLeft = false;
        }

        m_currentDpadValue = dpadValue;

        if (dpadLeft)
        {
            Debug.Log("test::左が押された");
            // dpadLeft = false;
        }
        else if (dpadRight)
        {
            Debug.Log("test::右が押された");
            //dpadRight = false;
        }
    }


}
