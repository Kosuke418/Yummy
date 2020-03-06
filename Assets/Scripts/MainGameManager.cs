using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    // 各種タイル
    Tile[,] AllTiles = new Tile[10, 10];
    List<Tile> EmptyTiles = new List<Tile>();
    // タイマー
    int second;
    int csecond;
    public Text timerText;
    public float totalTime;
    float setTime = 5f;
    // プレイヤーの所持食材と料理
    public static int[] Player1Ingred = new int[10];
    public static int[] Player2Ingred = new int[10];
    public static List<int> Player1Food = new List<int>();
    public static List<int> Player2Food = new List<int>();
    public Image[] madeFood1;
    public Image[] madeFood2;
    // 競売に使うスタティック変数
    public static int GameProgress;
    public static int PrecedNum;
    public static int IngredNum;
    public static int IngredNum2;
    // ターン
    public static int TurnCount = 1;
    int setTurnCount = 1;
    // プレイヤーの点数
    public static int P1Score = 1000;
    public static int P2Score = 1000;
    public Text scoreText1;
    public Text scoreText2;
    // 0がingred 1がfood
    List<int[]>[] reachNum = new List<int[]>[3];
    // ターンやスコアUIのテキストオブジェクト
    public Text TurnCountText;
    public Text P1ReachScore1;
    public Text P1ReachScore2;
    public Text P2ReachScore1;
    public Text P2ReachScore2;
    // リーチ中の食べ物のスコア
    public static string P1ReachScore;
    public static string P2ReachScore;

    public Image IngredTileLeft;
    public Image IngredTileRight;
    // オーディオデータ
    public AudioClip StartVoice;
    public AudioClip select;
    public AudioClip Make1Food;
    public AudioClip Make2Food;
    public AudioClip PutFood;
    public AudioSource seSource;
    public AudioSource bgmSource;

    Dialog dialog;
    InputManager inputManager;

    public enum State
    {
        Ready,
        Generate,
        Choice,
        MakeFood,
        Check,
        CheckTile,
        Result,
        ScoreView
    }

    private State state;

    void Awake()
    {
        //スタティック初期化
        reachNum[1] = new List<int[]>();
        reachNum[2] = new List<int[]>();

        inputManager = GetComponent<InputManager>();
        dialog = GetComponent<Dialog>();
        // 残り食材の表示
        TurnCountText.text = "残り食材." + TurnCount;
        // PlayerScoreの表示
        scoreText1.text = P1Score.ToString();
        scoreText2.text = P2Score.ToString();
    }


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

        // Auctionから帰ってきたときGameProgress=1としてState.MakeFoodに遷移
        if (GameProgress == 1)
        {
            EnableIngredandFood();
            // 取得時に満タンの時最初の食材から消す
            ArrangeTiles();
            if (PrecedNum == 1)
            {
                SelectGetIngred(1);
            }
            else
            {
                SelectGetIngred(2);
            }
            state = State.CheckTile;
        }
        // Auctionから掛け金0で帰ってきたときGameProgress=2としてState.MakeFoodに遷移
        else if (GameProgress == 2)
        {
            EnableIngredandFood();
            state = State.Generate;
        }
        // 通常状態においてState.Readyに遷移
        else
        {
            state = State.Ready;
        }
    }

    // 食材生成の関数(numは生成する食料数の指定)
    void Generate(int num)
    {
        //startからendまでの食料を生成
        var start = 1;
        var end = 19;

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

    // タイルの並び替えの関数(隙間が空いたら詰める)
    void CheckTiles()
    {
        for (int j = 0; j < 9; j++)
        {
            for (int i = 0; i < 9; i++)
            {
                if (AllTiles[1, i].Number == 0)
                {
                    Player1Ingred[i] = Player1Ingred[i + 1];
                    Player1Ingred[i + 1] = 0;
                    AllTiles[1, i].Number = AllTiles[1, i + 1].Number;
                    AllTiles[1, i + 1].Number = 0;
                }
                if (AllTiles[2, i].Number == 0)
                {
                    Player2Ingred[i] = Player2Ingred[i + 1];
                    Player2Ingred[i + 1] = 0;
                    AllTiles[2, i].Number = AllTiles[2, i + 1].Number;
                    AllTiles[2, i + 1].Number = 0;
                }
            }
        }
    }

    void ArrangeTiles()
    { 
        if (AllTiles[1, 9].Number != 0)
        {
            AllTiles[1, 0].Number = 0;
            Player1Ingred[0] = 0;
        }
        if (AllTiles[2, 9].Number != 0)
        {
            AllTiles[2, 0].Number = 0;
            Player2Ingred[0] = 0;
        }
        CheckTiles();
    }

    void PlayMakeFoodSound()
    {
        // 料理作成時のSEの判定、1/5の確率で包丁の音がなる
        int random = Random.Range(0, 5);
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
    }

    void CheckFood()
    {
        for (int n = 1; n < 3; n++)
        {
            var tempTileNum = new List<int>();
            var tempIngredNum = new List<int>();
            var reachFoodIndex = 0;
            var tempReachIngred = new int[2];
            reachNum[n] = new List<int[]>();

            foreach (Food f in Library.Instance.Foods)
            {
                if (f.Answers.Length == 0) continue;
                tempTileNum.Clear();
                tempIngredNum.Clear();
                tempReachIngred = new int[2];
                foreach (Answer a in f.Answers)
                {
                    for (int l = 0; l < 10; l++)
                    {
                        if (a.AnswerNumber == AllTiles[n, l].Number)
                        {
                            tempTileNum.Add(l);
                            break;
                        }
                        if (l == 9) tempIngredNum.Add(a.AnswerNumber);
                    }
                }
                // すべてのIngredThisがTrueの場合料理(Foods[i])が作れると判断
                if (f.Answers.Length == tempTileNum.Count())
                {
                    if (n == 1)
                    {
                        // 料理に使った手持ち食材に0を代入して消去
                        foreach (int i in tempTileNum)
                        {
                            AllTiles[1, i].Number = 0;
                            Player1Ingred[i] = 0;
                        }
                    }
                    if (n == 2)
                    {
                        // 料理に使った手持ち食材に0を代入して消去
                        foreach (int i in tempTileNum)
                        {
                            AllTiles[2, i].Number = 0;
                            Player2Ingred[i] = 0;
                        }
                    }

                    // FoodScoreをPlayerScoreに代入
                    if (n == 1) P1Score += f.FoodScore;
                    if (n == 2) P2Score += f.FoodScore;
                    // PlayerScoreTextの更新
                    scoreText1.text = P1Score.ToString();
                    scoreText2.text = P2Score.ToString();

                    PlayMakeFoodSound();

                    // 出来上がった料理の画像をmadeFoodに代入する
                    if (n == 1)
                    {
                        for (int i = 0; i < Player1Food.Count(); i++)
                        {
                            if (madeFood1[i].sprite == null && Player1Food[i] == 0)
                            {
                                madeFood1[i].enabled = true;
                                madeFood1[i].sprite = f.FoodSprite;
                                Player1Food[i] = i;
                                break;
                            }
                        }
                    }
                    if (n == 2)
                    {
                        for (int i = 0; i < Player2Food.Count(); i++)
                        {
                            if (madeFood2[i].sprite == null && Player2Food[i] == 0)
                            {
                                madeFood2[i].enabled = true;
                                madeFood2[i].sprite = f.FoodSprite;
                                Player2Food[i] = i;
                                break;
                            }
                        }
                    }
                }
                else if (tempIngredNum.Count() == 1)
                {
                    tempReachIngred[0] = tempIngredNum[0];
                    tempReachIngred[1] = reachFoodIndex;
                    reachNum[n].Add(tempReachIngred);
                }
                reachFoodIndex++;
            }
        }
    }

    void ReachCheck()
    {
        float level = 0.90f * Mathf.Abs(Mathf.Sin(Time.time * 3));
        float G0, G1;
        if (reachNum[1].Count > 0)
        {
            // 左側タイルのP1リーチ判定
            foreach (int[] i in reachNum[1])
            {
                Debug.Log(1);
                if (i[0] == AllTiles[0, 0].Number)
                {
                    // 赤色点滅
                    IngredTileLeft.GetComponent<Image>().color = new Color(1f, level, level, 1f);
                    P1ReachScore1.enabled = true;
                    P1ReachScore1.text = "+" + Library.Instance.Foods[i[1]].FoodScore.ToString();
                    break;
                }
                else
                {
                    // 無色
                    P1ReachScore1.text = "";
                    IngredTileLeft.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                }
            }
            // 右側タイルのP1リーチ判定
            foreach (int[] i in reachNum[1])
            {
                if (i[0] == AllTiles[0, 1].Number)
                {
                    IngredTileRight.GetComponent<Image>().color = new Color(1f, level, level, 1f);
                    P1ReachScore2.enabled = true;
                    P1ReachScore2.text = "+" + Library.Instance.Foods[i[1]].FoodScore.ToString();
                    break;
                }
                else
                {
                    P1ReachScore2.text = "";
                    IngredTileRight.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
        else
        {
            // 無色
            P1ReachScore1.text = "";
            IngredTileLeft.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            P1ReachScore2.text = "";
            IngredTileRight.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        }
        if (reachNum[2].Count > 0)
        {
            // 左側タイルのP2リーチ判定
            foreach (int[] i in reachNum[2])
            {
                if (i[0] == AllTiles[0, 0].Number)
                {
                    // すでに赤色に点滅する判定を受けていた場合紫色に点滅
                    if (IngredTileLeft.GetComponent<Image>().color.g >= 0.9f)
                    {
                        // 紫色に点滅
                        IngredTileLeft.GetComponent<Image>().color = new Color(1f, level, 1f, 1f);
                    }
                    else
                    {
                        //青色に点滅
                        IngredTileLeft.GetComponent<Image>().color = new Color(level, level, 1f, 1f);
                    }
                    P2ReachScore1.enabled = true;
                    P2ReachScore1.text = "+" + Library.Instance.Foods[i[1]].FoodScore.ToString();
                    break;
                }
                else
                {
                    P2ReachScore1.text = "";
                }
            }
            // 右側タイルのP2リーチ判定
            foreach (int[] i in reachNum[2])
            {
                if (i[0] == AllTiles[0, 1].Number)
                {
                    if (IngredTileRight.GetComponent<Image>().color.g >= 0.9f)
                    {
                        IngredTileRight.GetComponent<Image>().color = new Color(1f, level, 1f, 1f);
                    }
                    else
                    {
                        IngredTileRight.GetComponent<Image>().color = new Color(level, level, 1f, 1f);
                    }
                    P2ReachScore2.enabled = true;
                    P2ReachScore2.text = "+" + Library.Instance.Foods[i[1]].FoodScore.ToString();
                    break;
                }
                else
                {
                    P2ReachScore2.text = "";
                }
            }
        }
    }

    //1なら左がP1 2なら左がP2
    void SelectGetIngred(int num)
    {
        int right;
        if (num == 1) right = 2;
        else right = 1;
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[num, FoodNum].Number == 0 && Player1Ingred[FoodNum] == 0)
            {
                AllTiles[num, FoodNum].Number = AllTiles[0, 0].Number;
                Player1Ingred[FoodNum] = AllTiles[0, 0].Number;
                AllTiles[0, 0].Number = 0;

                seSource.clip = PutFood;
                seSource.Play();

                break;
            }
        }
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[right, FoodNum].Number == 0 && Player2Ingred[FoodNum] == 0)
            {
                AllTiles[right, FoodNum].Number = AllTiles[0, 1].Number;
                Player2Ingred[FoodNum] = AllTiles[0, 1].Number;
                AllTiles[0, 1].Number = 0;
                break;
            }
        }
    }

    void EnableIngredandFood()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Player1Ingred[i] != 0)
                AllTiles[1, i].Number = Player1Ingred[i];
            if (Player2Ingred[i] != 0)
                AllTiles[2, i].Number = Player2Ingred[i];
        }
        for (int i = 0; i < Player1Food.Count(); i++)
        {
            if (Player1Food[i] != 0)
            {
                madeFood1[i].enabled = true;
                madeFood1[i].sprite = Library.Instance.Foods[Player1Food[i]].FoodSprite;
            }
        }
        for (int i = 0; i < Player2Food.Count(); i++)
        {
            if (Player2Food[i] != 0)
            {
                madeFood2[i].enabled = true;
                madeFood2[i].sprite = Library.Instance.Foods[Player2Food[i]].FoodSprite;
            }
        }
    }
    
    void Update()
    {
        // ここからゲームの流れに沿ってStateが遷移
        // ゲームのスタート判定State、isPlayingならばState.Generateに遷移
        if (state == State.Ready)
        {
            if (dialog.IsPlaying()) state = State.Generate;
        }
        // 食材を生成するState
        else if (state == State.Generate)
        {
            // 真ん中のタイルに食材が入っていなかった場合自動的にGenerateを行うその後Choiceに遷移
            if (AllTiles[0, 0].Number == 0 && AllTiles[0, 1].Number == 0)
            {
                Generate(2);
                TurnCount--;
                // 残り食材の表示
                TurnCountText.text = "残り食材." + TurnCount;
                totalTime = setTime;
                state = State.Choice;
            }
        }
        // 食材を選択する際のState
        else if (state == State.Choice)
        {
            ReachCheck();

            // タイマーの表示とスタート
            totalTime -= Time.deltaTime;
            second = (int)totalTime;
            csecond = (int)((totalTime - second) * 100);
            if (second >= 0 && csecond >= 0)
            {
                timerText.text = second.ToString() + "." + csecond.ToString();
            }

            if (totalTime <= 0)
            {
                state = State.MakeFood;
            }
            GameProgress = 0;
        }
        else if (state == State.MakeFood)
        {
            if (inputManager.GetCursorPosition(1) == 0 && inputManager.GetCursorPosition(2) == 0)
            {
                P1ReachScore = P1ReachScore1.text;
                P2ReachScore = P2ReachScore1.text;
                IngredNum = AllTiles[0, 0].Number;
                IngredNum2 = AllTiles[0, 1].Number;
                SceneManager.LoadScene("AuctionScene");
            }
            else if (inputManager.GetCursorPosition(1) == 1 && inputManager.GetCursorPosition(2) == 1)
            {
                P1ReachScore = P1ReachScore2.text;
                P2ReachScore = P2ReachScore2.text;
                IngredNum = AllTiles[0, 1].Number;
                IngredNum2 = AllTiles[0, 0].Number;
                SceneManager.LoadScene("AuctionScene");
            }
            else if (inputManager.GetCursorPosition(1) == 0 && inputManager.GetCursorPosition(2) == 1)
            {
                ArrangeTiles();
                SelectGetIngred(1);

            }
            else if (inputManager.GetCursorPosition(1) == 1 && inputManager.GetCursorPosition(2) == 0)
            {
                ArrangeTiles();
                SelectGetIngred(2);
            }
            state = State.CheckTile;
        }
        else if (state == State.CheckTile)
        {
            CheckFood();
            CheckTiles();
            if (TurnCount == 0)
            {
                state = State.Result;
            }
            else
            {
                state = State.Generate;
            }
        }
        else if (state == State.Result)
        {
            dialog.ResultDialog();
            Player1Ingred = new int[10];
            Player2Ingred = new int[10];
            Player1Food = new List<int>();
            Player2Food = new List<int>();
            timerText.enabled = false;
            TurnCount = setTurnCount;

            if (Input.GetKeyDown(KeyCode.Space))
            {

                SceneManager.LoadScene("Result");
            }
        }
    }
}