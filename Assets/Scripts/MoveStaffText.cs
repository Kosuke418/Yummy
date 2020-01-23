using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveStaffText : MonoBehaviour
{
    [SerializeField]
    GameObject StaffText, Filter;
    [SerializeField]
    AudioClip soundDone;
    AudioSource audioEnd;
    float i = -1250;
    float R, G, B, A;
    int j = 0;



    void Start()
    {
        StaffText.transform.position = new Vector3(0, i, 0);

        audioEnd = GetComponent<AudioSource>();
        R = Filter.GetComponent<SpriteRenderer>().color.r;
        G = Filter.GetComponent<SpriteRenderer>().color.g;
        B = Filter.GetComponent<SpriteRenderer>().color.b;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 7"))
        {
            SceneManager.LoadScene("Title");
        }
        if (i < 930)
        {
            i += 2.0f;
            StaffText.transform.position = new Vector3(0, i, 0);
        }
        else if(j==0)
        {
            j++;
            audioEnd.PlayOneShot(soundDone);
        }
        /*else if (j == 20)
        {
            j++;
            Filter.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0, 0, 0.5f);
        }
        */
        else if(j > 20)
        {
            SceneManager.LoadScene("Title");
        }
        else
        {
            j++;
        }
    }
}
