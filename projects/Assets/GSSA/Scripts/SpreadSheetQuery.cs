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
    public class SpreadSheetQuery
    {
        private readonly List<CompareData> _compareList = new List<CompareData>();
        private readonly string sheetName;

        public IEnumerable<SpreadSheetObject> Result { private set; get; }
        public int Count { private set; get; }

        public SpreadSheetQuery(string sheetName = null)
        {
            this.sheetName = sheetName ?? SpreadSheetSetting.Instance.DefalutSheetName;
        }

        private int? _limit;
        private int? _skip;
        private string _orderKey;
        private bool _isDesc;

        public SpreadSheetQuery Limit(int? limit = null)
        {
            _limit = limit;
            return this;
        }

        public SpreadSheetQuery Skip(int? skip = null)
        {
            _skip = skip;
            return this;
        }

        public SpreadSheetQuery OrderByAscending(string key)
        {
            _orderKey = key;
            _isDesc = false;
            return this;
        }

        public SpreadSheetQuery OrderByDescending(string key)
        {
            _orderKey = key;
            _isDesc = true;
            return this;
        }

        public SpreadSheetQuery ClearOrderBy()
        {
            _orderKey = null;
            _isDesc = true;
            return this;
        }

        public SpreadSheetQuery Where(string target, CompareData.CompareType op, object value)
        {
            _compareList.Clear();
            return AndWhere(target, op, value);
        }


        public SpreadSheetQuery Where(string target, string op, object value)
        {
            _compareList.Clear();
            return AndWhere(target, op, value);
        }

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

        public SpreadSheetQuery AndWhere(string target, CompareData.CompareType op, object value)
        {
            var compare = new CompareData{target = target, value = value, compare = op};
            if (compare.compare != CompareData.CompareType.NONE) _compareList.Add(compare);
            return this;
        }

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