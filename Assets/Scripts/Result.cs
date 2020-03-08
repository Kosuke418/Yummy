﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    public Text ResultText;
    public GameObject Player1;
    public GameObject Player2;
    public GameObject hikiwake1;
    public GameObject hikiwake2;

    public float totalTime;

    // Start is called before the first frame update
    void Start()
    {
        totalTime = 5f;
        /*
        if (MainGameManager.player1Money > MainGameManager.player2Money)
        {
            Player1.GetComponent<SpriteRenderer>().enabled = true;
            ResultText.text = "P1の勝ち！";

        }
        else if (MainGameManager.player1Money < MainGameManager.player2Money)
        {
            Player2.GetComponent<SpriteRenderer>().enabled = true;
            ResultText.text = "P2の勝ち！";
        }
        else
        {
            hikiwake1.GetComponent<SpriteRenderer>().enabled = true;
            hikiwake2.GetComponent<SpriteRenderer>().enabled = true;
            ResultText.text = "引き分け！！";
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        totalTime -= Time.deltaTime;

        if (totalTime <= 0)
        {
            SceneManager.LoadScene("STAFF");
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 7"))
        {
            SceneManager.LoadScene("Title");
        }
    }
}
