using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<int> intlist = new List<int> { 1, 5, 4, 3 };
        List<int> orderedList = intlist.OrderBy(i => i).ToList();
        orderedList.ForEach(i => Debug.Log(i.ToString()));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
