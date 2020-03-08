using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVariable : MonoBehaviour
{
    public int playerNumber;
    public int[] havingIngredientNos = new int[10];
    public List<int> havingCuisineNos = new List<int>();
    public Image[] havingCuisineImages;
    public Text havingScoreText;
    public List<int> reachIngredientNos = new List<int>();
	public List<int> reachCuisineNos = new List<int>();
	public Text leftReachCuisineScore;
    public Text rightReachCuisineScore;

    public int HavingScore
    {
        get
        {
            return havingScore;
        }

        set
        {
            havingScore = value;
            havingScoreText.text = value.ToString();
        }
    }

    private int havingScore;

    // Start is called before the first frame update
    void Start()
    {
        HavingScore = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
