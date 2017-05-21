## 1.下準備
- 必要なもの
    - Googleのアカウント

・GoogleSpreadSheetのページへ行く https://www.google.com/intl/ja_jp/sheets/about/

・GoogleSpreadSheetを使う　をクリック
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/97f1ae2c-d10d-c9ee-618f-bcad11ad66c2.png)



・右下の＋をクリックでシートを追加
![image.png](https://qiita-image-store.s3.amazonaws.com/0/37184/0051da6a-9f69-1ebc-74e2-03dcb5ad2b28.png)


・シートの画面が開くので、「ツール」→「スクリプトエディタ」
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/d8203ce2-62cf-e079-1e5c-b550c290f5dd.png)

・function myFunction() {}　は消して、コードをペタっとな (最新はこちら→https://github.com/divide-by-zero/GSSA/blob/master/projects/Assets/GSSA/GoogleAppsScript/GSSA.gs)

・ 「公開」→「Webアプリケーションとして導入...」 → プロジェクト名の編集　（好きな名前を。そのままでも良いです）
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/401c3a04-81d8-6ecb-61c0-af121942d6d1.png)

・プロジェクトバージョンは「新規作成」　アプリケーションにアクセスできるユーザーは「全員（匿名ユーザーを含む）」にして「導入」
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/5f946e78-310d-c542-4665-377bdaf7fbd8.png)

・「許可が必要です」のダイアログが表示されるので、「許可を確認」→ 自分のGoogleアカウントを選んで「許可」
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/afd25e0b-6a16-e3af-478c-0b56e941706d.png)

・「現在のウェブアプリケーションのURL」　を**コピーして取っておいてください。**
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/1016984d-2dff-8c1c-0ea7-e89f8aab4cd7.png)

GoogleSpreadSheet上の作業は以上です。　簡単に。というわりに手順が多めですが最初の1回だけですし、あまり迷うところがないのでそんな苦じゃないかと。

## 2.Unity側準備

github(https://github.com/divide-by-zero/GSSA) のReleaseフォルダから GSSA.unitypackage をダウンロード(直リンク：https://github.com/divide-by-zero/GSSA/raw/master/Release/GSSA.unitypackage) し、
使いたいプロジェクトにインポートしてください。
（メニューのAssets→Import Package→Custom PackageからGSSA.unitypackageを選択してImportするか、プロジェクトを起動した状態でGSSA.unitypackageダブルクリックでも大丈夫（なはず））
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/1fd61aa9-8a00-17f1-aa0e-731f838ec63f.png)

1番最初のシーンにGSSA/Prefabs/SpreadSheetSetting prefabを配置
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/2d35668e-cc8e-bccd-4fbf-595fff669c39.png)
**※空のGameObjectを配置して、GSSA/Scripts/SpreadSheetSettingsを自分でアタッチしても良いです**

inspectorのurlの枠に↑でコピーしておいた**「現在のウェブアプリケーションのURL」**をセット
![image](https://qiita-image-store.s3.amazonaws.com/0/37184/ec5394f6-7d1e-3c6f-11fd-8d94d42dbf44.png)

**Is Debug Log Output** はチェックを入れると微妙に通信のログが出力されます。デバッグ時にどうぞ。
**Default Sheet Name** はとりあえず入れなくても良いです（後述）

これで準備完了です！　さぁ使いましょう。

## 3.データの保存

NiftyのmBasSに似た感じになってます（ちょっと参考にしつつ、不要だと思ったところをそぎ落としたので似ているだけです）

例えば、空のGameObjectに適当なScriptをアタッチして、Startにでもこんな感じで書きます

```csharp
using UnityEngine;

using GSSA;
public class GSSATest : MonoBehaviour {
    void Start ()
    {
        var so = new SpreadSheetObject("Chat");
        so["name"] = "かつーき";
        so["message"] = "たべないでください！";
        so.SaveAsync();
    }
}

```

これで実行をすると、先ほど作ったスプレッドシートに「Chat」というタブが追加され、1行目にnameとmessageとcreateTime(これだけは勝手に作られます）、2行目にそれぞれプログラムで指定した文字列と作成日時の数字が記入されているはずです。
![image.png](https://qiita-image-store.s3.amazonaws.com/0/37184/d4869da9-c6b9-9823-6d1f-bde902942cbd.png)


もうお分かりですね。　GoogleSpreadSheetを使って簡易なKey-Value-Storeを用意しました。
Google Apps ScriptにはdoPostかdoGetというメソッドを用意することで、Webサービス風にふるまう事が出来るので、それをインタフェースにSpreadSheetを操作しているだけです。

## 4.データの取得

なお、これではSpreadSheetにデータを追加しているだけなので、取得もやってみます。

```csharp
using System.Linq;
using UnityEngine;

using GSSA;
public class GSSATest : MonoBehaviour {
    void Start()
    {
        var query = new SpreadSheetQuery("Chat");
        query.Where("name", "=", "かつーき");
        query.FindAsync(list => {
            foreach (var so in list)
            {
                Debug.Log(so["name"] + ">" + so["message"]);
            }
        });
    }
}
```
![image.png](https://qiita-image-store.s3.amazonaws.com/0/37184/ee2966f0-6614-b113-2e25-07c108484663.png)

これまたNiftyのmBasSよろしく、Query発行のために```SpreadSheetQuery```クラスをnewしています。
なお、コンストラクタに渡しているのはシート名なんですが、「2.Unity側準備」でセットアップした**SpreadSheetSettingsのDefault Sheet Name**に基本的に使用するシート名を記述しておけば省略(nullを指定)した場合そちらが使われます（SpreadSheetObjectも同じです。　無事伏線回収）

また、```Whereメソッド```で絞り込みができます。　AND条件のみ対応しており、その場合には```AndWhereメソッド```を使ってください。それぞれのメソッドが自分自身を返すのでメソッドチェーンで書く事も出来ます。

＜例＞

```csharp
query.Where("name","=","かつーき").AndWhere("message","!=","もがもが").FindAsync(list=>{});
```
（なおOR条件はややこしくなるので省きました。要望があれば追加する・・？かも・・・？）

ソースがくっついているので、詳しくはソース見てね！って感じもしますが、一応補足。
Whereの第一引数はKeyになる項目。　第二引数は比較式です。=,==,<,<=,>,>=,!=,<> あたりだけ対応しています。

他には
- ```Limit(int count)```　…　返却されるリストの先頭から指定した数を上限として取得
- ```Skip(int count)```　…　返却されるリストの先頭から指定した数を飛ばして取得
- ```OrderByAscending(string key)```　…　返却されるリストを指定したkeyで昇順にソート
- ```OrderByDescending(string key)```　…　返却されるリストを指定したkeyで降順にソート

があります。

FindAsyncの引数にはcallbackで`Action<List<SpreadSheetObject>>`が書けるようになっており、戻ってきたList<SpreadSheetObject>に対しての処理をラムダ式で記述する感じです。

そして、何気にFindAsync（SaveAsyncも）はTaskのasync-awaitのように、コルーチンの中であればyield return で待機可能になっています。
具体的には↓な感じ

```csharp
using System.Collections;
using System.Linq;
using UnityEngine;

using GSSA;
public class GSSATest : MonoBehaviour {
    void Start ()
    {
	    StartCoroutine(ChatLogGetIterator());
    }

    private IEnumerator ChatLogGetIterator()
    {
        var query = new SpreadSheetQuery("Chat");
        query.Where("name", "=", "かつーき");
        yield return query.FindAsync();

        foreach (var so in query.Result)
        {
            Debug.Log(so["name"] + ">" + so["message"]); 
        }
    }
}
```
この場合はQueryオブジェクトの```Result```に```List<SpreadSheetObject>```が格納されます。(こっちの方が使い良いかも？)
なお、Listの件数はQueryオブジェクトのCountに格納されます。
使っても使わなくてもなんですが、SpreadSheetQueryには```CountAsync```メソッドも用意してあり、こっちは件数だけを返却してくれるので、自分のスコアが何位なのか、などをそれなり高速に調べるのに使えます。

## 5.データの更新
データの更新はどうするのか。　というと、SpreadSheetQueryで取得できたSpreadSheetObjectに対して値をセットして、SaveAsyncを呼ぶだけです。

```csharp
using System.Collections;
using System.Linq;
using UnityEngine;
using GSSA;

public class GSSATest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ChatLogGetIterator());
    }

    private IEnumerator ChatLogGetIterator()
    {
        var query = new SpreadSheetQuery("Chat");
        query.Where("name", "=", "かつーき");
        yield return query.FindAsync();

        var so = query.Result.FirstOrDefault();
        if (so != null)
        {
            so["message"] = "たべないよ！";
            yield return so.SaveAsync();
        }
    }
}
```

SpreadSheetを見るとちゃんと更新されています。
![image.png](https://qiita-image-store.s3.amazonaws.com/0/37184/e92abec0-196f-8df1-fdf3-5d0d79b22f24.png)

内部的にはobjectIdというid（というか、SpreadSheetでの行番号)を持っていてそれの有り無しで新規か更新か分岐している感じです。
