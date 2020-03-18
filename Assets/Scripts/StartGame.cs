using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public Text Press;
    private float level;

    void Start()
    {

    }

    void Update()
    {
        level = Mathf.Abs(Mathf.Sin(Time.time * 10));
        Press.color = new Color(0f, 0f, 0f, level);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 7"))
        {
            StartCoroutine(StartGameDelay(0.5f));
        }
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            StartCoroutine(StartGameDelay(0.5f));
        }
    }

    private IEnumerator StartGameDelay(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Main");
    }
}