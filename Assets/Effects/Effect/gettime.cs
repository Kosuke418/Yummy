using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gettime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float level = Mathf.PingPong(Time.time, 1.0F);
        Debug.Log(level);
        this.GetComponent<Image>().material.SetFloat("_Vector1_D08391B4", level);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.A))

            // this.GetComponent<Image>().material.GetFloat(Time.time) 
            float level = Mathf.PingPong(Time.time, 1.0F);
            Debug.Log(level);
            this.GetComponent<Image>().material.SetFloat("_Vector1_D08391B4", level);
        }
        */
    }
}
