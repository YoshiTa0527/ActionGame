using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstSelectButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Selectable sel = GetComponent<Selectable>();
        sel.Select();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
