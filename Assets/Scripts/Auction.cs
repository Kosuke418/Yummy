using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Auction : MonoBehaviour
{
    [SerializeField]
    Text p1ScoreText, p2ScoreText, p1BidValueText, p2BidValueText, auctionTimer, lbText, rbText, p1FoodPoint, p2FoodPoint, bidTimeReCast;
    //各プレイヤーの得点、入札額、タイマーの変数
    [SerializeField]
    GameObject P1Sprite, P2Sprite, Curtain1, Curtain2, BronzCoin, GoldCoin;
    [SerializeField]
    Image foodImage;
    [SerializeField]
    Image[] endImage = new Image[2];
    [SerializeField]
    private Text[] endText = new Text[1];
    [SerializeField]
    AudioClip soundMoney1, soundMoney2, audioWood, soundQuartzer;
    AudioSource audioMoney1;
    int player, kamenRiderKuronos, kamenRiderZeroOne, pcount, coinCount, charaCount, bidCount, p1BidCount, p2BidCount, countQuartzer, lowBid, highBid;//playerは現在の先行プレイヤーと入札ターンのプレイヤーの判定に使う。
    int[] textCase = new int[2];
    int[] CoinYet=new int[2];
    float R, G, B, kamenRider, charaTransform, curtainTransform, x, y, z, p1TenCount, p2TenCount;

    void Start()
    {
        //初期化一覧
        x = 50;
        y = -40;
        z = 100;
        p1BidCount = 0;
        p2BidCount = 0;
        p1TenCount = 0;
        p2TenCount = 0;
        charaCount = 1;
        coinCount = 0;
        bidCount = 6;
        pcount = 0;
        kamenRider = 5;
        curtainTransform = 150;
        lowBid = 10;
        highBid = 50;
        countQuartzer = 0;
        CoinYet[0] = 10;
        CoinYet[1] = 10;

        Debug.Log("スタート");
        
        //結果表示画面を不可視化
        for (int count = 0; count < 2; count++)
        {
            R = endImage[count].GetComponent<Image>().color.r;
            G = endImage[count].GetComponent<Image>().color.g;
            B = endImage[count].GetComponent<Image>().color.b;
            endImage[count].GetComponent<Image>().color = new Color(R, G, B, 0.0f);
        }
        R = endText[0].GetComponent<Text>().color.r;
        G = endText[0].GetComponent<Text>().color.g;
        B = endText[0].GetComponent<Text>().color.b;
        endText[0].GetComponent<Text>().color = new Color(R, G, B, 0.0f);

        p1BidValueText.text = "0";
        p2BidValueText.text = "0";

        //プレイヤースコアテキストを設定
        p1ScoreText.text = (MainGameManager2.P1Score).ToString();
        p2ScoreText.text = (MainGameManager2.P2Score).ToString();

        //操作テキストを設定
        lbText.text = "+" + lowBid.ToString();
        rbText.text = "+" + highBid.ToString();

        //ここから食材画像を指定
        foodImage.sprite = Library.Instance.Ingreds[MainGameManager2.IngredNum].IngredSprite;

        
        //AudioSourceConmponentを取得
        audioMoney1 = GetComponent<AudioSource>();

        //時間回復回数の表示
        bidTimeReCast.text = "時間回復回数\nあと" + bidCount + "回";

        //食料がリーチの時点数を表示
        p1FoodPoint.text = MainGameManager2.P1ReachScore;
        p2FoodPoint.text = MainGameManager2.P2ReachScore;
    }

    private void Update()
    {
        CharaMove();
        kamenRider -= Time.deltaTime;
        kamenRiderKuronos = (int)kamenRider;
        kamenRiderZeroOne = (int)((kamenRider - kamenRiderKuronos) * 100);
        auctionTimer.text = kamenRiderKuronos.ToString() + ":" + kamenRiderZeroOne;
        TimeQuartzer();
        if (pcount == 0)
        {
            Controler();
            if (kamenRider <= 0)
            {

                textCase[0] = int.Parse(p1BidValueText.text);
                textCase[1] = int.Parse(p2BidValueText.text);
                if (textCase[0] < textCase[1])
                {
                    MainGameManager2.P2Score -= int.Parse(p2BidValueText.text);
                    MainGameManager2.PrecedNum = 2;
                    EndAuction();
                }
                else if (textCase[0] > textCase[1])
                {
                    MainGameManager2.P1Score -= int.Parse(p1BidValueText.text);
                    MainGameManager2.PrecedNum = 1;
                    EndAuction();
                }
                else if (textCase[0] == textCase[1])
                {
                    //kamenRider = 3;
                    MainGameManager2.GameProgress = 2;
                    MainGameManager2.TurnCount--;
                    SceneManager.LoadScene("Main");

                }
            }
        }
        else if (pcount == 1)
        {
            curtainTransform -= 1.05f;
            Curtain1.transform.position = new Vector3(-curtainTransform, 0, 100);
            Curtain2.transform.position = new Vector3(curtainTransform, 0, 100);
            if (coinCount % 3 == 0)
            {
                audioMoney1.PlayOneShot(soundMoney2);
                coinCount+=1;
            }
            if (kamenRider <= 0) {
                MainGameManager2.GameProgress = 1;
                SceneManager.LoadScene("Main");
            }
        }
    }

    private void BidCheck(int bNum, int bidPlayer)
    {
        textCase[0] = int.Parse(p1BidValueText.text);
        textCase[1] = int.Parse(p2BidValueText.text);
        if (bNum == 0)
        {
            if (bidPlayer == 1 && textCase[0] <= textCase[1])
            {
                CoinCreate(CoinYet[0], 1);
                p1BidValueText.text = (textCase[1] + lowBid).ToString();
                audioMoney1.PlayOneShot(soundMoney1);
                CoinCreate(bNum, bidPlayer);
                CoinYet[0] = bNum;
                CoinYet[1] = bidPlayer;
                TimeReCast();
            }
            else if (bidPlayer == 2 && textCase[0] >= textCase[1])
            {
                CoinCreate(CoinYet[0], 2);
                p2BidValueText.text = (textCase[0] + lowBid).ToString();
                audioMoney1.PlayOneShot(soundMoney1);
                CoinCreate(bNum, bidPlayer);
                CoinYet[0] = bNum;
                CoinYet[1] = bidPlayer;
                TimeReCast();
            }
        }
        else if (bNum == 1)
        {
            if (bidPlayer == 1 && textCase[0] <= textCase[1])
            {
                CoinCreate(CoinYet[0], 1);
                p1BidValueText.text = (textCase[1] + highBid).ToString();
                audioMoney1.PlayOneShot(soundMoney2);
                CoinCreate(bNum, bidPlayer);
                CoinYet[0] = bNum;
                CoinYet[1] = bidPlayer;
                TimeReCast();
            }
            else if (bidPlayer == 2&& textCase[0] >= textCase[1])
            {
                CoinCreate(CoinYet[0], 2);
                p2BidValueText.text = (textCase[0] + highBid).ToString();
                audioMoney1.PlayOneShot(soundMoney2);
                CoinCreate(bNum, bidPlayer);
                CoinYet[0] = bNum;
                CoinYet[1] = bidPlayer;
                TimeReCast();
            }   
        }
    }

    public void EndAuction()
    {
        //▼ここに入札者名、入札額を表示するプログラム▼
        //結果表示画面を可視化
        for (int n = 0; n < 2; n++)
        {
            R = endImage[n].GetComponent<Image>().color.r;
            G = endImage[n].GetComponent<Image>().color.g;
            B = endImage[n].GetComponent<Image>().color.b;
            endImage[n].GetComponent<Image>().color = new Color(R, G, B, 1.0f);
            Debug.Log(R + " " + G + " " + B);
        }
        R = endText[0].GetComponent<Text>().color.r;
        G = endText[0].GetComponent<Text>().color.g;
        B = endText[0].GetComponent<Text>().color.b;
        endText[0].GetComponent<Text>().color = new Color(R, G, B, 1.0f);

        if (MainGameManager2.PrecedNum == 1)
        {
            endText[0].text = "Player1が\n" + textCase[0].ToString() + "点で\n落札しました";
        }else if (MainGameManager2.PrecedNum == 2)
        {
            endText[0].text = "Player2が\n" + textCase[1].ToString() + "点で\n落札しました";
        }
        //▲ここに入札者名、入札額を表示するプログラム▲

        //操作テキストの更新
        lbText.text = "";
        rbText.text = "";

        //SEを流すプログラム
        audioMoney1.PlayOneShot(audioWood);

        pcount = 1;

        //時間経過を表示を表示
        kamenRider = 2.5f;
    }

    void CharaMove()
    {
       // charaTransform = Mathf.Abs(Mathf.Sin(Time.time * 10));
        if (charaCount > 0)
        {
            charaTransform += 0.1f;
            P1Sprite.transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            P2Sprite.transform.eulerAngles = new Vector3(0, 0, charaTransform);
            if (charaTransform >= 5)
            {
                charaCount=-1;
            }
        }else if (charaCount < 0)
        {
            charaTransform -= 0.1f;
            P1Sprite.transform.eulerAngles = new Vector3(0, 0, -charaTransform);
            P2Sprite.transform.eulerAngles = new Vector3(0, 0, charaTransform);
            if (charaTransform <= -10)
            {
                charaCount = 1;
            }
        }
    }
    //▲キャラ動作▲

    void Controler()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown("joystick 1 button 2")/* || Input.GetKeyDown("joystick 1 button 4")*/)
        {
            BidCheck(0, 1);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown("joystick 1 button 1")/*|| Input.GetKeyDown("joystick 1 button 5")*/)
        {
            BidCheck(1, 1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown("joystick 2 button 2")/* || Input.GetKeyDown("joystick 2 button 4")*/)
        {
            BidCheck(0, 2);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown("joystick 2 button 1")/*|| Input.GetKeyDown("joystick 2 button 5")*/)
        {
            BidCheck(1, 2);
        }
    }

    void CoinCreate(int bidOR, int playerNumber)
    {
        if (bidOR != 10 || playerNumber != 10)
        {
            if (bidOR == 0)
            {
                if (playerNumber == 1)
                {
                    Instantiate(BronzCoin, new Vector3(-(x - (4 * p1TenCount)), y + (3 * p1BidCount), z), Quaternion.identity);
                    p1BidCount++;
                }
                else if (playerNumber == 2)
                {
                    Instantiate(BronzCoin, new Vector3(x - (4 * p2TenCount), y + (3 * p2BidCount), z), Quaternion.identity);
                    p2BidCount++;
                }
            }
            else if (bidOR == 1)
            {
                if (playerNumber == 1)
                {
                    Instantiate(GoldCoin, new Vector3(-(x - (4 * p1TenCount)), y + (3 * p1BidCount), z), Quaternion.identity);
                    p1BidCount++;
                }
                else if (playerNumber == 2)
                {
                    Instantiate(GoldCoin, new Vector3(x - (4 * p2TenCount), y + (3 * p2BidCount), z), Quaternion.identity);
                    p2BidCount++;
                }
            }
            if (p1BidCount == 14)
            {
                p1BidCount = 0;
                p1TenCount++;
            }
            if (p2BidCount == 14)
            {
                p2BidCount = 0;
                p2TenCount++;
            }
        }
    }

    void TimeReCast()
    {
        if (bidCount >= 1)
        {
            //時間追加のプログラム:最大でも3s追加は3回, 2s追加は2回, 1s追加は1回
            if (kamenRider <= 3 && bidCount >= 4)
            {
                kamenRider = 3;
                bidCount--;
                bidTimeReCast.text = "時間回復回数\nあと" + bidCount + "回";
                countQuartzer = 1;
            }
            else if (kamenRider <= 2 && bidCount >= 2)
            {
                kamenRider = 2;
                bidCount--;
                bidTimeReCast.text = "時間回復回数\nあと" + bidCount + "回";
                countQuartzer = 2;
            }
            else if (kamenRider <= 1 && bidCount >= 1)
            {
                kamenRider = 1;
                bidCount--;
                bidTimeReCast.text = "時間回復回数\nあと" + bidCount + "回";
            }
        }
    }

    void TimeQuartzer()
    {
        if (kamenRider <= 3.1f && countQuartzer==0)
        {
            audioMoney1.PlayOneShot(soundQuartzer);
            countQuartzer++;//1になる(次は2秒)
        }else if (kamenRider <= 2.1f && countQuartzer==1)
        {
            audioMoney1.PlayOneShot(soundQuartzer);
            countQuartzer++;//2になる
        }else if (kamenRider <= 1.1f && countQuartzer==2)
        {
            audioMoney1.PlayOneShot(soundQuartzer);
            countQuartzer++;//3になる
        }
    }

}