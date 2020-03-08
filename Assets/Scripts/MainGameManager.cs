using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    // 各種タイル
    public Tile[,] AllTiles = new Tile[3, 10];

    // タイマー
    int seconds;
    int commaSeconds;
    public Text timerText;
    public float limitTime;
    float selectionRestrictionTime = 1f;

    // 競売に使うスタティック変数
    public static int[] player1IngredientNos;
    public static int[] player2IngredientNos;
    public static List<int> player1CuisineNos;
    public static List<int> player2CuisineNos;
    public static int player1Score;
    public static int player2Score;
    public static int howToReturnFromAuction = 0;
    public static int preferredPlayerNo;
    public static int leftIngredientNo;
    public static int rightIngredientNo;

    // ターン
    int totalTurnCount = 99;
    public static int remainingTurnCount = 99;

    // ターンやスコアUIのテキストオブジェクト
    public Text TurnCountText;
    public Text P1ReachScore1;
    public Text P1ReachScore2;
    public Text P2ReachScore1;
    public Text P2ReachScore2;

    // リーチ中の食べ物のスコア
    //public static string P1ReachScore;
    //public static string P2ReachScore;

    // 中央二つのタイル
    public Image[] selectIngredientTiles = new Image[2];

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

    // プレイヤーごとの変数格納場所
    public GameObject[] playerSprite;
    PlayerVariable[] playerVariable = new PlayerVariable[3];


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
        for (int i = 1; i < playerVariable.Length; i++)
        {
            playerVariable[i] = playerSprite[i].GetComponent<PlayerVariable>();
        }
        inputManager = GetComponent<InputManager>();
        dialog = GetComponent<Dialog>();
        // 残り食材の表示
        TurnCountText.text = "残り食材." + remainingTurnCount;
        // PlayerScoreの表示
    }


    void Start()
    {
        // すべてのタイルNumberに0を代入する(タイルの初期化)
        var AllTilesOneDim = FindObjectsOfType<Tile>();
        foreach (Tile t in AllTilesOneDim)
        {
            t.Number = 0;
            AllTiles[t.PlayerNo, t.TileNo] = t;
        }
        // Auctionから帰ってきたときGameProgress=1としてState.MakeFoodに遷移
        if (howToReturnFromAuction == 1)
        {
            AssignVariableFromAuction();
            // 取得時に満タンの時最初の食材から消す
            ArrangeTiles();
            if (preferredPlayerNo == 1)
            {
                AllTiles[0, 0].Number = leftIngredientNo;
                AllTiles[0, 1].Number = rightIngredientNo;
                GetSelectIngred(1);
            }
            else
            {
                AllTiles[0, 0].Number = leftIngredientNo;
                AllTiles[0, 1].Number = rightIngredientNo;
                GetSelectIngred(2);
            }
            state = State.CheckTile;
        }
        // Auctionから掛け金0で帰ってきたときGameProgress=2としてState.MakeFoodに遷移
        else if (howToReturnFromAuction == 2)
        {
            AssignVariableFromAuction();
            state = State.Generate;
        }
        // 通常状態においてState.Readyに遷移
        else
        {
            player1IngredientNos = new int[10];
            player2IngredientNos = new int[10];
            state = State.Ready;
        }
    }

    // 食材生成の関数(numは生成する食料数の指定)
    void Generate(int num)
    {
        //startからendまでの食料を生成
        var start = 1;
        var end = Library.Instance.Ingreds.Count() - 1;

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
    void SortTiles()
    {
        for (int i = 1; i <= 2; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (AllTiles[i,j].Number == 0)
                {
                    for (int k = j + 1; k < 10; k++)
                    {
                        if (AllTiles[i, k].Number != 0)
                        {
                            AllTiles[i, j].Number = AllTiles[i, k].Number;
                            AllTiles[i, k].Number = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    void ArrangeTiles()
    {
        for (int i = 1; i <= 2; i++)
        {
            if (AllTiles[i, 9].Number != 0) AllTiles[i, 0].Number = 0;
        }
        SortTiles();
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
        for(int playerNo = 1; playerNo <= 2; playerNo++)
        {
            playerVariable[playerNo].reachIngredientNos.Clear();
            playerVariable[playerNo].reachCuisineNos.Clear();
            var tempTileNum = new List<int>();
            //var MissingIngredNums = new List<int>();

            for (int i = 0; i < Library.Instance.Foods.Count(); i++)
            {
                tempTileNum.Clear();
                //MissingIngredNums.Clear();

                for (int j = 0; j < Library.Instance.Foods[i].Answers.Count(); j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (Library.Instance.Foods[i].Answers[j].AnswerNumber == AllTiles[playerNo, k].Number)
                        {
                            tempTileNum.Add(k);
                            break;
                        }
                        /*
                        else if (k == 9)
                        {
                            MissingIngredNums.Add(Library.Instance.Foods[i].Answers[j].AnswerNumber);
                        }
                        */
                    }
                }

                /*
                if(MissingIngredNums.Count() == 1 && AllTiles[playerNo, 9].Number != 0)
                {
                    MissingIngredNums.Clear();
                    for (int j = 0; j < Library.Instance.Foods[i].Answers.Count(); j++)
                    {
                        for (int k = 1; k < 10; k++)
                        {
                            if (Library.Instance.Foods[i].Answers[j].AnswerNumber == AllTiles[playerNo, k].Number) break;
                            if (k == 9) MissingIngredNums.Add(Library.Instance.Foods[i].Answers[j].AnswerNumber);
                        }
                    }
                }
                */

                // 必要な食材数と持っている必要食材数が一致(料理する)
                if (Library.Instance.Foods[i].Answers.Count() == tempTileNum.Count() && tempTileNum.Count() != 0)
                {

                    playerVariable[playerNo].reachCuisineNos.Add(i);

                    foreach (int num in tempTileNum)
                    {
                        AllTiles[playerNo, num].Number = 0;
                    }

                    playerVariable[playerNo].HavingScore += Library.Instance.Foods[i].FoodScore;

                    PlayMakeFoodSound();

                    playerVariable[playerNo].havingCuisineNos.Add(i);

                    for (int j = 0; j < playerVariable[playerNo].havingCuisineImages.Length; j++)
                    {
                        if (playerVariable[playerNo].havingCuisineImages[j].sprite == null)
                        {
                            playerVariable[playerNo].havingCuisineImages[j].enabled = true;
                            playerVariable[playerNo].havingCuisineImages[j].sprite = Library.Instance.Foods[i].FoodSprite;
                            break;
                        }
                    }
                }
                /*
                // 足りない食材が残り一つの時(リーチ状態)
                else if (MissingIngredNums.Count() == 1)
                {
                    Debug.Log(tempTileNum[0] + "," + AllTiles[playerNo, 9].Number);
                    playerVariable[playerNo].reachIngredientNos.Add(MissingIngredNums[0]);
                    playerVariable[playerNo].reachCuisineNos.Add(i);
                }
                */
            }
        }
    }

    void CheckReachFood()
    {
        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            /*
            playerVariable[playerNo].reachIngredientNos.Clear();
            playerVariable[playerNo].reachCuisineNos.Clear();
            var tempTileNum = new List<int>();
            */
            var MissingIngredNums = new List<int>();

            for (int i = 0; i < Library.Instance.Foods.Count(); i++)
            {
                //tempTileNum.Clear();
                MissingIngredNums.Clear();
                
                for (int j = 0; j < Library.Instance.Foods[i].Answers.Count(); j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (k == 0 && AllTiles[playerNo, 9].Number != 0) continue;
                        if (Library.Instance.Foods[i].Answers[j].AnswerNumber == AllTiles[playerNo, k].Number)
                        {
                            //tempTileNum.Add(k);
                            break;
                        }
                        else if (k == 9)
                        {
                            MissingIngredNums.Add(Library.Instance.Foods[i].Answers[j].AnswerNumber);
                        }
                    }
                }

                // 足りない食材が残り一つの時(リーチ状態)
                if (MissingIngredNums.Count() == 1)
                {
                    playerVariable[playerNo].reachIngredientNos.Add(MissingIngredNums[0]);
                    playerVariable[playerNo].reachCuisineNos.Add(i);
                }
            }
        }
    }



    void CheckReach()
    {
        float level = 0.90f * Mathf.Abs(Mathf.Sin(Time.time * 3));
        bool[,] isReachIngredTile = new bool[2,3];
        int[,] tempIndexNumber = new int[2,3];
        Image tileColor;

        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            for(int i = 0; i < playerVariable[playerNo].reachIngredientNos.Count(); i++)
            {
                if (AllTiles[0, 0].Number == playerVariable[playerNo].reachIngredientNos[i])
                {
                    isReachIngredTile[0, playerNo] = true;
                    tempIndexNumber[0,playerNo] = i;
                }
                if (AllTiles[0, 1].Number == playerVariable[playerNo].reachIngredientNos[i])
                {
                    isReachIngredTile[1, playerNo] = true;
                    tempIndexNumber[1,playerNo] = i;
                }
            }
        }

        for(int i = 0; i < 2; i++)
        {
            tileColor = selectIngredientTiles[i].GetComponent<Image>();
            if (isReachIngredTile[i,1])
            {
                // 赤色
                if (!isReachIngredTile[i,2])
                {
                    tileColor.color = new Color(1f, level, level, 1f);
                    //P1ReachScore1.enabled = true;
                    //P1ReachScore1.text = "+" + Library.Instance.Foods[playerVariable[1].reachCuisineNos[tempIndexNumber[i,1]]].FoodScore.ToString();
                }
                // 紫色
                else
                {
                    tileColor.color = new Color(1f, level, 1f, 1f);
                    //P1ReachScore1.enabled = true;
                    //P1ReachScore1.text = "+" + Library.Instance.Foods[playerVariable[1].reachCuisineNos[tempIndexNumber[i, 1]]].FoodScore.ToString();
                    //P1ReachScore1.enabled = true;
                    //P1ReachScore1.text = "+" + Library.Instance.Foods[playerVariable[1].reachCuisineNos[tempIndexNumber[i, 1]]].FoodScore.ToString();
                }
            }
            else if (isReachIngredTile[i,2])
            {
                // 青色
                tileColor.color = new Color(level, level, 1f, 1f);
            }
            else
            {
                // 白
                tileColor.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    //1なら左がP1 2なら左がP2
    void GetSelectIngred(int num)
    {
        int right;
        if (num == 1) right = 2;
        else right = 1;
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[num, FoodNum].Number == 0)
            {
                AllTiles[num, FoodNum].Number = AllTiles[0, 0].Number;
                AllTiles[0, 0].Number = 0;

                seSource.clip = PutFood;
                seSource.Play();

                break;
            }
        }
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[right, FoodNum].Number == 0)
            {
                AllTiles[right, FoodNum].Number = AllTiles[0, 1].Number;
                AllTiles[0, 1].Number = 0;
                break;
            }
        }
    }

    void SaveVariableToAuction()
    {
        for(int playerNo = 1; playerNo <= 2; playerNo++)
        {
            for (int i = 0; i < 10; i++)
            {
                playerVariable[playerNo].havingIngredientNos[i] = AllTiles[playerNo, i].Number;
                player1IngredientNos[i] = playerVariable[1].havingIngredientNos[i];
                player2IngredientNos[i] = playerVariable[2].havingIngredientNos[i];
            }
        }
        player1CuisineNos = playerVariable[1].havingCuisineNos;
        player2CuisineNos = playerVariable[2].havingCuisineNos;
        player1Score = playerVariable[1].HavingScore;
        player2Score = playerVariable[2].HavingScore;
    }
    void AssignVariableFromAuction()
    {
        playerVariable[1].havingCuisineNos = player1CuisineNos;
        playerVariable[2].havingCuisineNos = player2CuisineNos;
        for (int i = 0; i < 10; i++)
        {
            AllTiles[1, i].Number = player1IngredientNos[i];
            AllTiles[2, i].Number = player2IngredientNos[i];
        }
        for(int i = 0; i < player1CuisineNos.Count(); i++)
        {

            playerVariable[1].havingCuisineImages[i].enabled = true;
            playerVariable[1].havingCuisineImages[i].sprite = Library.Instance.Foods[player1CuisineNos[i]].FoodSprite;
        }
        for (int i = 0; i < player2CuisineNos.Count(); i++)
        {
            playerVariable[2].havingCuisineImages[i].enabled = true;
            playerVariable[2].havingCuisineImages[i].sprite = Library.Instance.Foods[player2CuisineNos[i]].FoodSprite;
        }
        playerVariable[1].HavingScore = player1Score;
        playerVariable[2].HavingScore = player2Score;
    }
    
    void Update()
    {
        // ここからゲームの流れに沿ってStateが遷移
        // ゲームのスタート判定State、isPlayingならばState.Generateに遷移
        if (state == State.Ready)
        {
            if (dialog.IsPlaying())
            {
                playerVariable[1].HavingScore = 1000;
                playerVariable[2].HavingScore = 1000;
                state = State.Generate;
            }
        }
        // 食材を生成するState
        else if (state == State.Generate)
        {
            // 真ん中のタイルに食材が入っていなかった場合自動的にGenerateを行うその後Choiceに遷移
            if (AllTiles[0, 0].Number == 0 && AllTiles[0, 1].Number == 0)
            {
                Generate(2);
                remainingTurnCount--;
                // 残り食材の表示
                TurnCountText.text = "残り食材." + remainingTurnCount;
                limitTime = selectionRestrictionTime;
                state = State.Choice;
            }
        }
        // 食材を選択する際のState
        else if (state == State.Choice)
        {
            CheckReach();

            // タイマーの表示とスタート
            limitTime -= Time.deltaTime;
            seconds = (int)limitTime;
            commaSeconds = (int)((limitTime - seconds) * 100);
            if (seconds >= 0 && commaSeconds >= 0)
            {
                timerText.text = seconds.ToString() + "." + commaSeconds.ToString();
            }

            if (limitTime <= 0)
            {
                state = State.MakeFood;
            }
            howToReturnFromAuction = 0;
        }
        else if (state == State.MakeFood)
        {
            if (inputManager.GetCursorPosition(1) == 0 && inputManager.GetCursorPosition(2) == 0)
            {
                //P1ReachScore = P1ReachScore1.text;
                //P2ReachScore = P2ReachScore1.text;
                leftIngredientNo = AllTiles[0, 0].Number;
                rightIngredientNo = AllTiles[0, 1].Number;
                SaveVariableToAuction();
                SceneManager.LoadScene("AuctionScene");
            }
            else if (inputManager.GetCursorPosition(1) == 1 && inputManager.GetCursorPosition(2) == 1)
            {
                //P1ReachScore = P1ReachScore2.text;
                //P2ReachScore = P2ReachScore2.text;
                leftIngredientNo = AllTiles[0, 1].Number;
                rightIngredientNo = AllTiles[0, 0].Number;
                SaveVariableToAuction();
                SceneManager.LoadScene("AuctionScene");
            }
            else if (inputManager.GetCursorPosition(1) == 0 && inputManager.GetCursorPosition(2) == 1)
            {
                ArrangeTiles();
                GetSelectIngred(1);
            }
            else if (inputManager.GetCursorPosition(1) == 1 && inputManager.GetCursorPosition(2) == 0)
            {
                ArrangeTiles();
                GetSelectIngred(2);
            }
            state = State.CheckTile;
        }
        else if (state == State.CheckTile)
        {
            if (remainingTurnCount == 0)
            {
                state = State.Result;
            }
            else
            {
                CheckFood();
                CheckReachFood();
                SortTiles();
                state = State.Generate;
            }
        }
        else if (state == State.Result)
        {
            dialog.ResultDialog();
            player1CuisineNos = new List<int>();
            player2CuisineNos = new List<int>();
            timerText.enabled = false;
            remainingTurnCount = totalTurnCount;

            if (Input.GetKeyDown(KeyCode.Space))
            {

                SceneManager.LoadScene("Result");
            }
        }
    }
}