using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jjjd : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {     
        string dd = JsonConvert.SerializeObject(new Vector3(3f,4f,4f));
        Debug.Log(dd);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
