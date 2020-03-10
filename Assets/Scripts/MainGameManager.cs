using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    // 各種タイル
    Tile[,] AllTiles = new Tile[3, 10];

    // タイマー
    readonly float selectionRestrictionTime = 5f;
    float limitTime;
    public Text TimerText;

    // 競売に使うスタティック変数
    public static int[] Player1IngredientsNumber;
    public static int[] Player2IngredientsNumber;
    public static List<int> Player1FoodsNumber;
    public static List<int> Player2FoodsNumber;
    public static int Player1Score;
    public static int Player2Score;
    public static int HowToReturnFromAuction;
    public static int PreferredPlayerNo;
    public static int SelectedIngredientNumber;
    public static int NotSelectedIngredientNumber;

    // ターン
    readonly int totalTurnCount = 30;
    public static int RemainingTurnCount;
    public Text TurnCountText;

    // 中央二つのタイル(リーチ中のカラーを変える為)
    public Image[] SelectIngredientTiles = new Image[2];

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
    public GameObject[] PlayerSprite;
    PlayerVariable[] playerVariable = new PlayerVariable[3];

    

    public enum State
    {
        Ready,
        Generate,
        Choice,
        SetIngred,
        CheckIngred,
        Result,
    }

    private State state;

    void Awake()
    {
        for (int i = 1; i < playerVariable.Length; i++)
        {
            playerVariable[i] = PlayerSprite[i].GetComponent<PlayerVariable>();
        }

        inputManager = GetComponent<InputManager>();
        dialog = GetComponent<Dialog>();
    }


    void Start()
    {
        var AllTilesOneDim = FindObjectsOfType<Tile>();

        foreach (Tile t in AllTilesOneDim)
        {
            t.Number = 0;
            AllTiles[t.PlayerNo, t.TileNo] = t;
        }

        // Auctionから遷移した際，State.CheckIngredにします．
        if (HowToReturnFromAuction == 1)
        {
            ResotoreVariableFromAuction();
            // 取得時に満タンの時最初の食材から消す
            if (PreferredPlayerNo == 1)
            {
                AddIngredient(1);
            }
            else
            {
                AddIngredient(2);
            }
            state = State.CheckIngred;
        }
        // Auctionから掛け金0で遷移した際，State.Choiceにします．
        else if (HowToReturnFromAuction == 2)
        {
            ResotoreVariableFromAuction();
            state = State.Choice;
        }
        // 通常，State.Readyにします．
        else
        {
            state = State.Ready;
        }
    }

    /// <summary>
    /// ゲームの開始時に呼び出します．
    /// </summary>
    /// <remarks>
    /// AuctionSceneから帰ってきた際に初期化したくない変数の初期化などに使用します．
    /// </remarks>
    void LoadStartVariable()
    {
        HowToReturnFromAuction = 0;
        playerVariable[1].Score = 1000;
        playerVariable[2].Score = 1000;
        Player1IngredientsNumber = new int[10];
        Player2IngredientsNumber = new int[10];
        Player1FoodsNumber = new List<int>();
        Player2FoodsNumber = new List<int>();
        RemainingTurnCount = totalTurnCount;
        TurnCountText.text = "残り食材." + RemainingTurnCount;
    }

    /// <summary>
    /// 中央のTileにIngredientを生成します．
    /// </summary>
    /// <remarks>
    /// 生成したいIngredientの数を指定し，その数のIngredientを中央のTileに生成します．
    /// </remarks>
    /// <param name="numIngredient">生成したいIngredientの数を指定できます．</param>
    void GenerateIngredient(int numIngredient)
    {
        var start = 1;
        var end = Library.Instance.Ingreds.Count() - 1;

        List<int> numbers = new List<int>();

        for (int i = start; i <= end; i++)
        {
            numbers.Add(i);
        }
        while (numIngredient-- > 0)
        {
            int index = Random.Range(0, numbers.Count);
            int randomNum = numbers[index];
            AllTiles[0, numIngredient].Number = randomNum;
            numbers.RemoveAt(index);
        }
    }

    /// <summary>
    /// 指定の時間が経ったときTrueを返します．
    /// </summary>
    bool IsTimelimit()
    {
        // タイマーの表示とスタート
        limitTime -= Time.deltaTime;
        int seconds = (int)limitTime;
        int commaSeconds = (int)((limitTime - seconds) * 100);
        if (seconds >= 0 && commaSeconds >= 0)
        {
            TimerText.text = seconds.ToString() + "." + commaSeconds.ToString();
        }
        return limitTime <= 0f;
    }

    /// <summary>
    /// 指定されたTileのIngredientをそれぞれの手持ちIngredientに格納します．
    /// </summary>
    /// <param name="leftPlayerNo">左のTileを指定したプレイヤーのNoです．</param>
    void AddIngredient(int leftPlayerNo)
    {
        RemoveFirstIngredient();
        int rightPlayerNo = 3 - leftPlayerNo;
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[leftPlayerNo, FoodNum].Number == 0)
            {
                AllTiles[leftPlayerNo, FoodNum].Number = AllTiles[0, 0].Number;
                AllTiles[0, 0].Number = 0;

                seSource.clip = PutFood;
                seSource.Play();

                break;
            }
        }
        for (int FoodNum = 0; FoodNum < 10; FoodNum++)
        {
            if (AllTiles[rightPlayerNo, FoodNum].Number == 0)
            {
                AllTiles[rightPlayerNo, FoodNum].Number = AllTiles[0, 1].Number;
                AllTiles[0, 1].Number = 0;

                seSource.clip = PutFood;
                seSource.Play();

                break;
            }
        }
    }

    /// <summary>
    /// AucitionSceneに遷移する際に変数を保存します．
    /// </summary>
    void StoreVariableToAuction()
    {
        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            for (int i = 0; i < 10; i++)
            {
                playerVariable[playerNo].IngredientsNumber[i] = AllTiles[playerNo, i].Number;
            }
        }
        Player1IngredientsNumber = playerVariable[1].IngredientsNumber;
        Player2IngredientsNumber = playerVariable[2].IngredientsNumber;
        Player1FoodsNumber = playerVariable[1].FoodsNumber;
        Player2FoodsNumber = playerVariable[2].FoodsNumber;
        Player1Score = playerVariable[1].Score;
        Player2Score = playerVariable[2].Score;
    }
    /// <summary>
    /// AucitionSceneから遷移する際に変数を復元します．
    /// </summary>
    void ResotoreVariableFromAuction()
    {
        playerVariable[1].FoodsNumber = Player1FoodsNumber;
        playerVariable[2].FoodsNumber = Player2FoodsNumber;
        for (int i = 0; i < 10; i++)
        {
            AllTiles[1, i].Number = Player1IngredientsNumber[i];
            AllTiles[2, i].Number = Player2IngredientsNumber[i];
        }
        for (int i = 0; i < Player1FoodsNumber.Count(); i++)
        {
            playerVariable[1].FoodsImage[i].enabled = true;
            playerVariable[1].FoodsImage[i].sprite = Library.Instance.Foods[Player1FoodsNumber[i]].FoodSprite;
        }
        for (int i = 0; i < Player2FoodsNumber.Count(); i++)
        {
            playerVariable[2].FoodsImage[i].enabled = true;
            playerVariable[2].FoodsImage[i].sprite = Library.Instance.Foods[Player2FoodsNumber[i]].FoodSprite;
        }
        playerVariable[1].Score = Player1Score;
        playerVariable[2].Score = Player2Score;
        AllTiles[0, 0].Number = SelectedIngredientNumber;
        AllTiles[0, 1].Number = NotSelectedIngredientNumber;
    }

    /// <summary>
    /// Tileの間に隙間が空いた場合に詰めます．
    /// </summary>
    void SortIngredients()
    {
        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (AllTiles[playerNo,j].Number == 0)
                {
                    for (int k = j + 1; k < 10; k++)
                    {
                        if (AllTiles[playerNo, k].Number != 0)
                        {
                            AllTiles[playerNo, j].Number = AllTiles[playerNo, k].Number;
                            AllTiles[playerNo, k].Number = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tilesが一杯になっている場合は最初のTileを取り除きます．
    /// </summary>
    void RemoveFirstIngredient()
    {
        if (AllTiles[1, 9].Number != 0) AllTiles[1, 0].Number = 0;
        if (AllTiles[2, 9].Number != 0) AllTiles[2, 0].Number = 0;
        SortIngredients();
    }

    /// <summary>
    /// Foodを作る際のSEを再生します．
    /// </summary>
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

    /// <summary>
    /// 手持ちのingredientからFoodを生成します．
    /// </summary>
    /// <remarks>
    /// LibraryのFoodの順番で手持ちIngredientと比較し，
    /// Foodを生成できるIngredientを所持していた場合生成し，手持ちFoodに格納します．
    /// </remarks>
    void CookFood()
    {
        for(int playerNo = 1; playerNo <= 2; playerNo++)
        {
            playerVariable[playerNo].ReachIngredientsNumber.Clear();
            playerVariable[playerNo].ReachFoodsNumber.Clear();
            var tempTileNum = new List<int>();

            for (int i = 0; i < Library.Instance.Foods.Count(); i++)
            {
                tempTileNum.Clear();
                for (int j = 0; j < Library.Instance.Foods[i].Answers.Count(); j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (Library.Instance.Foods[i].Answers[j].AnswerNumber == AllTiles[playerNo, k].Number)
                        {
                            tempTileNum.Add(k);
                            break;
                        }
                    }
                }

                // 料理に必要な食材数と持っている必要食材数が一致するとき料理し，手持ち料理に格納します．
                if (Library.Instance.Foods[i].Answers.Count() == tempTileNum.Count() && tempTileNum.Any())
                {

                    playerVariable[playerNo].ReachFoodsNumber.Add(i);

                    foreach (int num in tempTileNum)
                    {
                        AllTiles[playerNo, num].Number = 0;
                    }

                    playerVariable[playerNo].Score += Library.Instance.Foods[i].FoodScore;

                    PlayMakeFoodSound();

                    playerVariable[playerNo].FoodsNumber.Add(i);

                    for (int j = 0; j < playerVariable[playerNo].FoodsImage.Length; j++)
                    {
                        if (playerVariable[playerNo].FoodsImage[j].sprite == null)
                        {
                            playerVariable[playerNo].FoodsImage[j].enabled = true;
                            playerVariable[playerNo].FoodsImage[j].sprite = Library.Instance.Foods[i].FoodSprite;
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 手持ちのIngredientからリーチ判定を行います．
    /// </summary>
    /// <remarks>
    /// LibraryのFoodの順番で手持ちIngredientと比較し，
    /// 足りないIngredientが残り一つ出会った時，それをReachIngredientsNumberに格納します．
    /// </remarks>
    void SaveReachIngredient()
    {
        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            var MissingIngredNums = new List<int>();

            for (int i = 0; i < Library.Instance.Foods.Count(); i++)
            {
                MissingIngredNums.Clear();
                
                for (int j = 0; j < Library.Instance.Foods[i].Answers.Count(); j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (k == 0 && AllTiles[playerNo, 9].Number != 0) continue;
                        if (Library.Instance.Foods[i].Answers[j].AnswerNumber == AllTiles[playerNo, k].Number)
                        {
                            break;
                        }
                        else if (k == 9)
                        {
                            MissingIngredNums.Add(Library.Instance.Foods[i].Answers[j].AnswerNumber);
                        }
                    }
                }

                // 足りない食材が残り一つの時リーチ食材として格納するします．
                if (MissingIngredNums.Count() == 1)
                {
                    playerVariable[playerNo].ReachIngredientsNumber.Add(MissingIngredNums[0]);
                    playerVariable[playerNo].ReachFoodsNumber.Add(i);
                }
            }
        }
    }

    /// <summary>
    /// リーチ判定中のIngredientが中央Tileに出現した場合に色を付けます．
    /// </summary>
    /// <remarks>
    /// SaveReachIngred()で保存したIngredientが中央Tileに出現した際，
    /// Player1であれば赤色，Player2であれば青色，両方のリーチIngredientであれば紫色に点滅します．
    /// </remarks>
    void PutColorToReachIngredient()
    {
        float level = 0.90f * Mathf.Abs(Mathf.Sin(Time.time * 3));
        bool[,] isReachIngredTile = new bool[2,3];
        int[,] tempIndexNumber = new int[2,3];
        Image tileColor;

        for (int playerNo = 1; playerNo <= 2; playerNo++)
        {
            for(int i = 0; i < playerVariable[playerNo].ReachIngredientsNumber.Count(); i++)
            {
                if (AllTiles[0, 0].Number == playerVariable[playerNo].ReachIngredientsNumber[i])
                {
                    isReachIngredTile[0, playerNo] = true;
                    tempIndexNumber[0,playerNo] = i;
                }
                if (AllTiles[0, 1].Number == playerVariable[playerNo].ReachIngredientsNumber[i])
                {
                    isReachIngredTile[1, playerNo] = true;
                    tempIndexNumber[1,playerNo] = i;
                }
            }
        }

        for(int i = 0; i < 2; i++)
        {
            tileColor = SelectIngredientTiles[i].GetComponent<Image>();
            if (isReachIngredTile[i,1])
            {
                // 赤色
                if (!isReachIngredTile[i,2])
                {
                    tileColor.color = new Color(1f, level, level, 1f);
                }
                // 紫色
                else
                {
                    tileColor.color = new Color(1f, level, 1f, 1f);
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
    
    void Update()
    {
        switch (state)
        {
            case State.Ready:
                if (dialog.IsPlaying())
                {
                    LoadStartVariable();
                    state = State.Generate;
                }

                break;

            case State.Generate:
                if (AllTiles[0, 0].Number == 0 && AllTiles[0, 1].Number == 0)
                {
                    GenerateIngredient(2);
                    RemainingTurnCount--;
                    TurnCountText.text = "残り食材." + RemainingTurnCount;
                    state = State.Choice;
                }
                limitTime = selectionRestrictionTime;

                break;

            case State.Choice:
                PutColorToReachIngredient();
                if (IsTimelimit()) state = State.SetIngred;
                break;

            case State.SetIngred:
                HowToReturnFromAuction = 0;
                if (inputManager.GetCursorPosition(1) == inputManager.GetCursorPosition(2))
                {
                    SelectedIngredientNumber = AllTiles[0, inputManager.GetCursorPosition(1)].Number;
                    NotSelectedIngredientNumber = AllTiles[0, 1 - inputManager.GetCursorPosition(1)].Number;
                    StoreVariableToAuction();
                    SceneManager.LoadScene("AuctionScene");
                }
                else if (inputManager.GetCursorPosition(1) != inputManager.GetCursorPosition(2))
                {
                    AddIngredient(1 + inputManager.GetCursorPosition(1));
                }
                state = State.CheckIngred;

                break;

            case State.CheckIngred:
                if (RemainingTurnCount != 0)
                {
                    CookFood();
                    SaveReachIngredient();
                    SortIngredients();
                    state = State.Generate;
                }
                else
                {
                    state = State.Result;
                }

                break;

            case State.Result:
                dialog.ResultDialog();
                TimerText.enabled = false;
                Player1Score = playerVariable[1].Score;
                Player2Score = playerVariable[2].Score;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SceneManager.LoadScene("Result");
                }

                break;
        }
    }
}