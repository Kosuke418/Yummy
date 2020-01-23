using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    // 各種タイル
    private Tile[,] AllTiles = new Tile[10, 10];
    private List<Tile> EmptyTiles = new List<Tile>();
    // プレイヤーのセレクトボタンの位置
    private int[] selectPos = { 0, 1 };
    // 画像
    public Image[] selectBotton = new Image[4];
    public Image[] madeFood = new Image[2];
    // プレイヤーの所持食材と料理
    public static int[,] PlayerIngred = new int[2, 10];
    public static int[,] PlayerFood = new int[2, 6];
    // ターン
    public static int TurnCount = 30;
    // プレイヤーの点数
    public static int[] PlayerScore = { 1000, 1000 };
    public Text[] scoreText = new Text[2];

    public static bool GenerateStop;
    private int random;
    public static int[] ReachIngred1 = new int[10];
    public static int[] ReachIngred2 = new int[10];
    int[] ReachFood1 = new int[10];
    int[] ReachFood2 = new int[10];
    public Text TurnCountText;
    public Text P1ReachScore1;
    public Text P1ReachScore2;
    public Text P2ReachScore1;
    public Text P2ReachScore2;
    public static string P1ReachScore;
    public static string P2ReachScore;
    public Image Dialog;
    public Text DialogText;


    public enum GameState
    {
        Ready,
        Generate,
        Choice,
        MakeFood,
        CheckTile,
        Result,
        ScoreView
    }
    private GameState state;

    void Start()
    {
        // すべてのタイルNumberに0を代入する(タイルの初期化)
        Tile[] AllTilesOneDim = GameObject.FindObjectsOfType<Tile>();
        foreach (Tile t in AllTilesOneDim)
        {
            t.Number = 0;
            AllTiles[t.IndCols, t.IndRows] = t;
            EmptyTiles.Add(t);
        }
        state = GameState.Ready;
        Dialog.enabled = true;
        DialogText.enabled = true;
        DialogText.text = "「STARTで調理スタート！！」";
    }

    // 食材生成の関数(numは生成する食料数の指定)
    void Generate(int num)
    {
        //startからendまでの食料を生成
        int start = 1;
        int end = 19;

        List<int> numbers = new List<int>();

        for (int i = start; i <= end; i++)
        {
            numbers.Add(i);
        }
        while (num-- > 0)
        {
            int index = Random.Range(0, numbers.Count);
            int randomNum = numbers[index];
            AllTiles[0, num].Number = randomNum;
            numbers.RemoveAt(index);
        }
    }

    private void PlayerMakeFood()
    {
        int[] Ingred1;
        bool[] IngredThis1 = new bool[10];
        int m1;
        int count1;
        int temp1;
        int reachCount1 = 0;
        ReachIngred1 = new int[10];
        ReachFood1 = new int[10];
        for (int i = 1; i < 14; i++)
        {
            m1 = 0;
            Ingred1 = new int[10];
            count1 = 0;
            temp1 = 0;
            for (int k = 0; k < 8; k++)
            {
                if (Library.Instance.Foods[i].Answers[k].AnswerNumber != 0)
                {
                    for (int l = 0; l < 10; l++)
                    {
                        if (Library.Instance.Foods[i].Answers[k].AnswerNumber == AllTiles[1, l].Number)
                        {
                            // 手持ち食材に当たり食材が存在している場合IngredThisをTrueにする
                            // Debug.Log(Library.Instance.Foods[i].FoodName + "の" + Library.Instance.Foods[i].Answers[k].AnswerNumber + "があるよ");
                            Ingred1[m1] = l;
                            IngredThis1[k] = true;
                            m1++;
                            break;
                        }
                        else if (l == 9)
                        {
                            // 手持ち食材に当たり食材が存在していない場合IngredThisをFalseにする
                            // Debug.Log(Library.Instance.Foods[i].FoodName + "の" + Library.Instance.Foods[i].Answers[k].AnswerNumber + "がないよ");
                            IngredThis1[k] = false;
                            count1++;
                            temp1 = k;
                        }
                    }
                }
                else
                {
                    IngredThis1[k] = true;
                }
            }

            // すべてのIngredThisがTrueの場合料理(Foods[i])が作れると判断
            if (IngredThis1[0] && IngredThis1[1] && IngredThis1[2] && IngredThis1[3] && IngredThis1[4] && IngredThis1[5] && IngredThis1[6] && IngredThis1[7])
            {
                // 料理に使った手持ち食材に0を代入して消去
                for (int n = 0; n < 8; n++)
                {
                    AllTiles[1, Ingred1[n]].Number = 0;
                    Player1Ingred[Ingred1[n]] = 0;
                }

                // FoodSocreをPlayerScoreに代入
                // Debug.Log(Library.Instance.Foods[i].FoodName + "を作った");
                P1Score += Library.Instance.Foods[i].FoodScore;

                // 料理作成時のSEの判定、1/5の確率で包丁の音がなる
                random = Random.Range(0, 5);
                if (random == 0)
                {
                    seSource.clip = Make1Food;
                    seSource.Play();
                }
                else
                {
                    seSource.clip = Make2Food;
                    seSource.Play();
                }

                // 出来上がった料理の画像をmadeFoodに代入する
                for (int FoodNum = 0; FoodNum < 6; FoodNum++)
                {
                    if (madeFood1[FoodNum].sprite == null && Player1Food[FoodNum] == 0)
                    {
                        madeFood1[FoodNum].enabled = true;
                        madeFood1[FoodNum].sprite = Library.Instance.Foods[i].FoodSprite;
                        Player1Food[FoodNum] = i;
                        break;
                    }
                }
            }

            // 必要な食材が残り一つであった場合のみReachIngredにリーチ状態の食材番号(AnswerNumber)を代入
            else if (count1 == 1)
            {
                // Debug.Log("プレイヤー１リーチ" + Library.Instance.Foods[i].FoodName + "," + Library.Instance.Foods[i].Answers[temp1].AnswerName);
                ReachIngred1[reachCount1] = Library.Instance.Foods[i].Answers[temp1].AnswerNumber;
                ReachFood1[reachCount1] = i;
                reachCount1++;
            }
        }
    }

    void Update()
    {
        
    }
}
