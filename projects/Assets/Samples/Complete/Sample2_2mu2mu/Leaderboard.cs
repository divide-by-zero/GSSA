using System.Collections;
using GSSA;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public UIFader fader;
    public Text top5rankText;
    public Text rivalRankText;
    public Button backButton;

    public void Start()
    {
        backButton.onClick.AddListener(() => fader.Show(false));
    }

    public void LoadLeaderBoard(int hiscore)
    {
        StartCoroutine(_LoadLeaderBoardIterator(hiscore));
    }

    private IEnumerator _LoadLeaderBoardIterator(int hiscore)
    {
        top5rankText.text = "now loading";
        rivalRankText.text = "now loading";

        fader.Show(true);

        //まずTop5の取得
        var topRankQuery = new SpreadSheetQuery("ScoreRanking");

        //ハイスコアを降順（大きい順）にして、Limit5にすることで、TOP5を取得
        yield return topRankQuery.OrderByDescending("hiscore").Limit(5).FindAsync(); 

        //取得できたデータをうまく整形しつつ表示
        top5rankText.text = "Top5\n";
        var dispRank = 0;
        foreach (var so in topRankQuery.Result)
        {
            var text = ++dispRank + "位\t" + so["hiscore"];
            //自分のスコアを赤くするために、idでチェック
            if (so["id"] as string == SpreadSheetSetting.Instance.UniqueID)
            {
                text = "<color=red>" + text + "</color>";
            }
            top5rankText.text += text + "\n";
        }

        //近傍スコア（ライバル）の表示処理
        //まずプレイヤーの順位を取得
        var playerRankingQuery = new SpreadSheetQuery("ScoreRanking");
        yield return playerRankingQuery.Where("hiscore", ">", hiscore).CountAsync();  //自分よりスコアが高いプレイヤーが何人いるか
        var rank = playerRankingQuery.Count;    //自分のスコアのランク（-1）取得

        //自分の順位を取得できたので、そこからライバルのスコア取得
        var neigborRankingQuery = new SpreadSheetQuery("ScoreRanking");

        //TOP5同様、降順＋Limit5 に加え、自分のRank-2をSkipすることで、自分のスコアの2つ上のユーザーから取得
        dispRank = Mathf.Max(0, rank - 2);
        yield return neigborRankingQuery.OrderByDescending("hiscore").Skip(dispRank).Limit(5).FindAsync();   

        //取得できたデータをうまく整形しつつ表示
        rivalRankText.text = "your rival\n";
        foreach (var so in neigborRankingQuery.Result)
        {
            var text = ++dispRank + "位\t" + so["hiscore"];
            if (so["id"] as string == SpreadSheetSetting.Instance.UniqueID)
            {
                text = "<color=red>" + text + "</color>";
            }
            rivalRankText.text += text + "\n";
        }
    }
}
