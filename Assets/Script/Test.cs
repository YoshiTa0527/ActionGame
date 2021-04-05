using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{
    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");//右が1
        float v = Input.GetAxisRaw("Vertical"); //上が1
        Debug.Log($"test::h:{h},v:{v}");

    }
}
