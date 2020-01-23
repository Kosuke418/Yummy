using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public Text Press;
    private float level;

    // Start is called before the first frame update
    void Start()
    {
        MainGameManager2.P1Score = 100;
        MainGameManager2.P2Score = 100;
        MainGameManager2.IngredNum=0;
        MainGameManager2.IngredNum2=0;
        MainGameManager2.GameProgress=0;
        MainGameManager2.PrecedNum=0;
        MainGameManager2.Player1Ingred = new int[10];
        MainGameManager2.Player2Ingred = new int[10];
    }

    // Update is called once per frame
    void Update()
    {
        level = Mathf.Abs(Mathf.Sin(Time.time * 10));
        Press.color = new Color(0f, 0f, 0f, level);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 7"))
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