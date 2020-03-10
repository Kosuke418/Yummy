using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVariable : MonoBehaviour
{
    public int[] IngredientsNumber = new int[10];
    public List<int> FoodsNumber = new List<int>();
    public Image[] FoodsImage;
    public Text ScoreText;
    public List<int> ReachIngredientsNumber = new List<int>();
	public List<int> ReachFoodsNumber = new List<int>();
	public Text LeftReachCuisineScore;
    public Text RightReachCuisineScore;

    /// <summary>
    /// Scoreプロパティです。
    /// </summary>
    /// <value>Scoreに値を代入するとScoreTextにも代入されます</value>
    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;
            ScoreText.text = value.ToString();
        }
    }

    private int score;
}
