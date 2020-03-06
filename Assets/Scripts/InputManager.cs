using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{

    [SerializeField] Image leftSelectCursor1;
    [SerializeField] Image rightSelectCursor1;
    [SerializeField] Image leftSelectCursor2;
    [SerializeField] Image rightSelectCursor2;

	[SerializeField] AudioSource seSource;
    [SerializeField] AudioClip select;

    int cursorPosition1;
    int cursorPosition2;

    Dialog dialog;

    void Start()
    {
        leftSelectCursor1.enabled = true;
        rightSelectCursor1.enabled = false;
        leftSelectCursor2.enabled = false;
        rightSelectCursor2.enabled = true;

        cursorPosition1 = 0;
        cursorPosition2 = 1;

        dialog = GetComponent<Dialog>();
    }

    void PlaySound()
    {
        seSource.clip = select;
        seSource.Play();
    }

    void Update()
    {
        if (dialog.IsPlaying())
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                leftSelectCursor1.enabled = true;
                rightSelectCursor1.enabled = false;
                cursorPosition1 = 0;
                PlaySound();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                leftSelectCursor1.enabled = false;
                rightSelectCursor1.enabled = true;
                cursorPosition1 = 1;
                PlaySound();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                leftSelectCursor2.enabled = true;
                rightSelectCursor2.enabled = false;
                cursorPosition2 = 0;
                PlaySound();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                leftSelectCursor2.enabled = false;
                rightSelectCursor2.enabled = true;
                cursorPosition2 = 1;
                PlaySound();
            }
        }
    }

    public int GetCursorPosition(int playerNum)
    {
        if (playerNum == 1)
            return cursorPosition1;
        else if (playerNum == 2)
            return cursorPosition2;
        else
            return 1;
    }
}
