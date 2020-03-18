using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static int enemyCursor;

    IEnumerator EasyAICoroutine()
    {
        int i;
        while (true)
        {
            i = Random.Range(1, 3);
            enemyCursor = i;
            if(MainGameManager.state == MainGameManager.State.SetIngred)
            {
                break;
            }
            yield return new WaitForSeconds(Random.Range(1f,2f));
        }
    }

    public void EasyAI()
    {
        enemyCursor = 0;
        StartCoroutine(EasyAICoroutine());
    }

    // Start is called before the first frame update
    void Start()
    {
        enemyCursor = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
