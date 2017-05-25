using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GSSA
{
    /// <summary>
    /// GoogleSpreadSheetのデータ（1行）を表すデータオブジェクト
    /// </summary>
    public class SpreadSheetObject : Dictionary<string, object>
    {
        private string sheetName;
        private int objectId = -1; //IDというか、行番号
        public static string Id { get { return SystemInfo.deviceUniqueIdentifier; } }

        /// <summary>
        /// 保持しているデータをintにして返却。
        /// 一旦文字列にしてからparseするので若干遅い
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int ParseInt(string key)
        {
            return int.Parse(this[key].ToString());
        }
        /// <summary>
        /// 保持しているデータをfloatにして返却。
        /// 一旦文字列にしてからparseするので若干遅い
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float ParseFloat(string key)
        {
            return float.Parse(this[key].ToString());
        }

        /// <summary>
        /// 保持しているデータをstringにして返却
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ParseString(string key)
        {
            return this[key].ToString();
        }

        /// <summary>
        /// コンストラクタ
        /// sheetNameを省略(null)にした場合は、SpreadSheetSettingのDefalutSheetNameを使用
        /// objectIdは基本指定しないが、あえて指定することで、SpreadSheetの行数を指定してデータを保持できる
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="objectId"></param>
        public SpreadSheetObject(string sheetName = null, int objectId = -1)
        {
            this.sheetName = sheetName ?? SpreadSheetSetting.Instance.DefalutSheetName;
            this.objectId = objectId;
        }

        /// <summary>
        /// 保存処理
        /// Coroutineの中であればyield returnで待機可能
        /// 返却値(int)はそのままobjectIdとして格納される。　負数の場合は保存処理失敗。
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public CustomYieldInstruction SaveAsync(Action<int> callback = null)
        {
            var complete = false;
            SpreadSheetSetting.Instance.Enqueue(()=>SaveAsyncIterator(callback,b => complete = b));
            return new WaitUntil(() => complete);
        }

        private IEnumerator SaveAsyncIterator(Action<int> callback,Action<bool> complete)
        {
            var form = new WWWForm();
            form.AddField(SpreadSheetConst.Method, "Save");
            form.AddField(SpreadSheetConst.SheetName, sheetName);
            form.AddField(SpreadSheetConst.ObjectId, objectId);
            foreach (var pair in this)
            {
                form.AddField(pair.Key, pair.Value.ToString());
            }

            using (var www = UnityWebRequest.Post(SpreadSheetSetting.Instance.SpreadSheetUrl, form))
            {
                yield return www.Send();
                if (SpreadSheetSetting.Instance.IsDebugLogOutput)
                {
                    Debug.Log("GSSA SaveAsync Response:\n" + www.downloadHandler.text);
                }
                var jsonNode = JsonNode.Parse(www.downloadHandler.text);
                objectId = jsonNode[SpreadSheetConst.ObjectId].GetInt();
                if(callback != null)callback.Invoke(objectId);
            }
            complete(true);
        }
    }
}
