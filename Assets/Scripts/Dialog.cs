using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    public Text dialogText;
    public Image dialogPanel;

    void Start()
    {
        if(MainGameManager.HowToReturnFromAuction == 0)
        StartDialog();
    }

    void StartDialog()
    {
        dialogPanel.enabled = true;
        dialogText.enabled = true;
        dialogText.text = "「SPACEで調理スタート！！」";
    }

    public void ResultDialog()
    {
        dialogPanel.enabled = true;
        dialogText.enabled = true;
        dialogText.text = "「SPACEで結果表示！！」";
    }

    public void PauseDialog()
    {
        dialogText.text = "「ポーズ！！」";
    }

    public void Pause()
    {
        // pauseがfalseの時true、trueの時falseに変更
        dialogPanel.enabled = !dialogPanel.enabled;
        dialogText.enabled = !dialogText.enabled;
        // pauseがtrueの時、タイムスケールを0にしてゲーム時間を止める
        if (dialogPanel.enabled)
        {
            PauseDialog();
            Time.timeScale = 0f;
        }
        // pauseがfalseの時、タイムスケールを1にしてゲーム時間を進める
        else
        {
            Time.timeScale = 1f;
        }
    }

    public bool IsPlaying()
    {
        return !dialogPanel.enabled;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogPanel.enabled = false;
            dialogText.enabled = false;
            dialogText.text = "";
        }
        // ゲーム中であるとき、Spaceキーが押されたらポーズ関数に移行
        if (Input.GetKeyDown(KeyCode.X))
        {
            Pause();
        }
    }
}
