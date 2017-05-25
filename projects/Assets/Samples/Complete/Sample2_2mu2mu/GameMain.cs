using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GSSA;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameMain : MonoBehaviour
{
	public Ball ballPrefab;
	private List<Ball> ballList = new List<Ball>();
	private float time = 10;
	private int score = 0;
	public Text scoreText;
	public Text modeText;
    public DialogPanel resultPanel;
    public Button retryButton;
    public Button leaderBoardButton;
    public Leaderboard leaderboard;

    

    private int HiScore
    {
        get { return PlayerPrefs.GetInt("HiScore"); }
        set { PlayerPrefs.SetInt("HiScore",value); }
    }

	enum GAME_MODE
	{
		START,
		PLAY,
		OVER,
        RESULT,
	}
	
	GAME_MODE mode = GAME_MODE.START;

	void Start ()
	{
		StartCoroutine(CrateBall());

	    retryButton.onClick.AddListener(() => SceneManager.LoadScene("2mu2mu"));
	    leaderBoardButton.onClick.AddListener(() => leaderboard.LoadLeaderBoard(HiScore));
	}

    private IEnumerator ScoreSendIterator()
    {
        leaderBoardButton.interactable = false; //スコア送信する前にリーダーボードを見ても自分のスコアが表示されないので無効化

        var isHiscore = false;
        if (score > HiScore)    //所持しているHiScoreよりも今回のScoreの方が大きい場合
        {
            HiScore = score;
            isHiscore = true;
            retryButton.interactable = false;   //すぐリトライできるとスコア送信する前にCoroutineが止められてしまうので無効化
        }

        //スコア結果のパネル表示
        resultPanel.Show("Time Attack Results","HiScore " + HiScore + "\nScore " + score + (isHiscore ? "\nハイスコア更新!!" : ""), "Time Attack の結果です。");

        if (isHiscore)
        {
            resultPanel.Description = "サーバーのハイスコアを確認しています。";

            //すでにスコアが登録されているかチェック
            var hiScoreCheck = new SpreadSheetQuery();
            yield return hiScoreCheck.Where("id", "=", SpreadSheetSetting.Instance.UniqueID).FindAsync();   //"id"を検索条件に入れることで、すでにスコアが登録されているかチェック

            //既にハイスコアは登録されている
            if (hiScoreCheck.Count > 0)
            {
                resultPanel.Description = "ハイスコアの更新処理中・・・";

                //登録されている＝hiScoreCheckの戻りリストが更新対象SpreadSheetObjectになるので、そのまま使用する
                var so = hiScoreCheck.Result.First();
                so["hiscore"] = HiScore;
                yield return so.SaveAsync();
            }
            else
            {
                resultPanel.Description = "ハイスコアの新規登録中・・・";

                //登録されていなかったので、新規としてidにUniqueIDを入れて次の更新処理に備えたデータで保存する
                var so = new SpreadSheetObject();
                so["id"] = SpreadSheetSetting.Instance.UniqueID;
                so["hiscore"] = HiScore;
                yield return so.SaveAsync();
            }
            resultPanel.Description = "サーバーへのハイスコア登録処理が終了しました。";
        }

        //ハイスコア登録処理が終わったので、リトライとリーダーボードへの遷移ボタンを有効化
        leaderBoardButton.interactable = true;
        retryButton.interactable = true;
    }

    private IEnumerator CrateBall()
	{
		for (float x = -2.5f; x <=2.5f; ++x) {
			for (int y = -5; y < 5; ++y)
			{
				var ball = Instantiate(ballPrefab);
				ball.transform.position = new Vector3(x, y, 0);
				ball.No = Random.Range(0, 4);
				ball.gameMain = this;
			}
		}
		while (true)
		{
			var ball = Instantiate(ballPrefab);
			ball.transform.position = new Vector3(Random.Range(-2.5f,2.5f), 6, 0);
			ball.No = Random.Range(0, 4);
			ball.gameMain = this;
			yield return new WaitForSeconds(0.5f);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		switch (mode)
		{
			case GAME_MODE.START:
				modeText.text = "2mu2mu\nClick to Start";
				if (Input.GetMouseButtonDown(0))
				{
					mode = GAME_MODE.PLAY;
				}
				break;
			case GAME_MODE.PLAY:
				modeText.text = "";
				PlayGame();
				break;
			case GAME_MODE.OVER:
				modeText.text = "GAME OVER";
			    StartCoroutine(ScoreSendIterator());
                mode = GAME_MODE.RESULT;
				break;
		    case GAME_MODE.RESULT:
		        break;
		    default:
		        throw new ArgumentOutOfRangeException();
		}
	}

	public void PlayGame()
	{
		if (Input.GetMouseButtonUp(0)) {
			foreach (var ball in ballList) {
				if (ballList.Count >= 3) {
					Destroy(ball.gameObject);
					score += ballList.Count * 10;
				}
				ball.IsDrag = false;
			}
			ballList.Clear();
		}
		scoreText.text = "TIME:" + (int)time +"\nHiScore:" + HiScore + "\nScore:" + score;
		time -= Time.deltaTime;
		if (time < 0)
		{
		    time = 0;
			mode = GAME_MODE.OVER;
		}
	}

	public void Register(Ball ball)
	{
		if (mode != GAME_MODE.PLAY) return;
		var lastBall = ballList.LastOrDefault();
		if (lastBall == null) lastBall = ball;

		if (ballList.Any(b => b == ball) == false && lastBall.No == ball.No && (lastBall.transform.position - ball.transform.position).magnitude < 1.5f)
		{
			ballList.Add(ball);
			ball.IsDrag = true;
		}
	}
}
