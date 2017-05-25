using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace GSSA
{
    /// <summary>
    /// GoogleSpreadSheetからデータ取得をするQueryオブジェクト
    /// </summary>
    public class SpreadSheetQuery
    {
        private readonly List<CompareData> _compareList = new List<CompareData>();
        private readonly string sheetName;

        public IEnumerable<SpreadSheetObject> Result { private set; get; }
        public int Count { private set; get; }

        /// <summary>
        /// コンストラクタ
        /// sheetNameを省略(null)にした場合は、SpreadSheetSettingのDefalutSheetNameを使用
        /// </summary>
        /// <param name="sheetName"></param>
        public SpreadSheetQuery(string sheetName = null)
        {
            this.sheetName = sheetName ?? SpreadSheetSetting.Instance.DefalutSheetName;
        }

        private int? _limit;
        private int? _skip;
        private string _orderKey;
        private bool _isDesc;

        /// <summary>
        /// 返却されるリストの先頭から指定した数を上限として取得
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public SpreadSheetQuery Limit(int? limit = null)
        {
            _limit = limit;
            return this;
        }

        /// <summary>
        /// 返却されるリストの先頭から指定した数を飛ばして取得
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public SpreadSheetQuery Skip(int? skip = null)
        {
            _skip = skip;
            return this;
        }

        /// <summary>
        /// 指定したキーで昇順にソートして返却
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SpreadSheetQuery OrderByAscending(string key)
        {
            _orderKey = key;
            _isDesc = false;
            return this;
        }

        /// <summary>
        /// 指定したキーで降順にソートして返却
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SpreadSheetQuery OrderByDescending(string key)
        {
            _orderKey = key;
            _isDesc = true;
            return this;
        }

        /// <summary>
        /// ソートキーのクリア処理
        /// </summary>
        /// <returns></returns>
        public SpreadSheetQuery ClearOrderBy()
        {
            _orderKey = null;
            _isDesc = true;
            return this;
        }

        /// <summary>
        /// 返却されるリストのフィルタ条件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SpreadSheetQuery Where(string target, CompareData.CompareType op, object value)
        {
            _compareList.Clear();
            return AndWhere(target, op, value);
        }

        /// <summary>
        /// 返却されるリストの検索条件
        /// op には =,==,<,<=,>,>=,!=,<> が使用可
        /// </summary>
        /// <param name="target"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SpreadSheetQuery Where(string target, string op, object value)
        {
            _compareList.Clear();
            return AndWhere(target, op, value);
        }

        /// <summary>
        /// AND検索条件
        /// op には =,==,<,<=,>,>=,!=,<> が使用可
        /// </summary>
        /// <param name="target"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SpreadSheetQuery AndWhere(string target, string op, object value)
        {
            var compareType = CompareData.CompareType.NONE;
            switch (op.Trim())
            {
                case "<":
                    compareType = CompareData.CompareType.LT;
                    break;
                case ">":
                    compareType = CompareData.CompareType.GT;
                    break;
                case "<=":
                    compareType = CompareData.CompareType.LE;
                    break;
                case ">=":
                    compareType = CompareData.CompareType.GE;
                    break;
                case "==":
                case "=":
                    compareType = CompareData.CompareType.EQ;
                    break;
                case "!=":
                case "<>":
                    compareType = CompareData.CompareType.NE;
                    break;
            }
            return AndWhere(target, compareType, value);
        }


        /// <summary>
        /// AND検索条件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SpreadSheetQuery AndWhere(string target, CompareData.CompareType op, object value)
        {
            var compare = new CompareData{target = target, value = value, compare = op};
            if (compare.compare != CompareData.CompareType.NONE) _compareList.Add(compare);
            return this;
        }

        /// <summary>
        /// 検索処理実行
        /// Coroutineの中であればyield returnで待機可能
        /// その場合の返却値はResultに格納される
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public CustomYieldInstruction FindAsync(Action<List<SpreadSheetObject>> callback = null)
        {
            var complete = false;
            SpreadSheetSetting.Instance.Enqueue(()=>FindAsyncIterator(callback,b => complete = b));
            return new WaitUntil(() => complete);
        }

        private IEnumerator FindAsyncIterator(Action<List<SpreadSheetObject>> callback,Action<bool> endAction)
        {
            var form = new WWWForm();
            form.AddField(SpreadSheetConst.Method, "Find");
            form.AddField(SpreadSheetConst.SheetName, sheetName);
            var output = Json.Serialize(_compareList.Select(data => data.ToDictionary()).ToList());
            form.AddField(SpreadSheetConst.Where, output);
            if(_skip.HasValue)form.AddField(SpreadSheetConst.Skip, _skip.Value);
            if(_limit.HasValue)form.AddField(SpreadSheetConst.Limit, _limit.Value);
            if (string.IsNullOrEmpty(_orderKey) == false)
            {
                form.AddField(SpreadSheetConst.OrderBy,_orderKey);
                form.AddField(SpreadSheetConst.IsDesc,_isDesc ? -1 : 1);
            }

            using (var www = UnityWebRequest.Post(SpreadSheetSetting.Instance.SpreadSheetUrl, form))
            {
                yield return www.Send();
                if (SpreadSheetSetting.Instance.IsDebugLogOutput)
                {
                    Debug.Log("GSSA FindAsync Response:\n" + www.downloadHandler.text);
                }
                var jsonNode = JsonNode.Parse(www.downloadHandler.text);

                var list = new List<SpreadSheetObject>();

                //ここで複数帰ってくる可能性がある
                var keys = jsonNode["keys"].Get<IList>();
                if (keys != null && keys.Count > 0)
                {
                    foreach (var findData in jsonNode["values"])
                    {
                        var sso = new SpreadSheetObject(sheetName, findData[SpreadSheetConst.ObjectId].GetInt());
                        for (var index = 0; index < keys.Count; index++)
                        {
                            var k = (string)keys[index];
                            var v = findData["value"][index].Get<object>();
                            sso[k] = v;
                        }
                        list.Add(sso);
                    }
                }
                Result = list;
                Count = list.Count;
                if (callback != null) callback(list);
                endAction(true);
            }
        }


        /// <summary>
        /// 検索したリストのカウントのみを取得
        /// Coroutineの中であればyield returnで待機可能
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public CustomYieldInstruction CountAsync(Action<int> callback = null)
        {
            var complete = false;
            SpreadSheetSetting.Instance.Enqueue(()=>CountAsyncIterator(callback,b => complete = b));
            return new WaitUntil(() => complete);
        }

        private IEnumerator CountAsyncIterator(Action<int> callback,Action<bool> endAction)
        {
            var form = new WWWForm();
            form.AddField(SpreadSheetConst.Method, "Count");
            form.AddField(SpreadSheetConst.SheetName, sheetName);
            var output = Json.Serialize(_compareList.Select(data => data.ToDictionary()).ToList());
            form.AddField(SpreadSheetConst.Where, output);

            using (var www = UnityWebRequest.Post(SpreadSheetSetting.Instance.SpreadSheetUrl, form))
            {
                yield return www.Send();
                if (SpreadSheetSetting.Instance.IsDebugLogOutput)
                {
                    Debug.Log("GSSA CountAsync Response:\n" + www.downloadHandler.text);
                }
                var jsonNode = JsonNode.Parse(www.downloadHandler.text);

                Count = jsonNode["Count"].GetInt();
                if (callback != null) callback(Count);
                endAction(true);
            }
        }

        [Serializable]
        public class CompareData
        {
            public enum CompareType
            {
                NONE,
                GT,
                LT,
                GE,
                LE,
                EQ,
                NE
            }
            public string target;

            public CompareType compare = CompareType.NONE;
            public object value;

            public Dictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>{{SpreadSheetConst.Target, target},{SpreadSheetConst.Value, value},{SpreadSheetConst.Compare, compare}};
            }
        }
    }
}