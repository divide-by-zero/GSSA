using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GSSA
{
    public class SpreadSheetObject : Dictionary<string, object>
    {
        private string sheetName;
        private int objectId = -1; //IDÇ∆Ç¢Ç§Ç©ÅAçsî‘çÜ
        public static string Id { get { return SystemInfo.deviceUniqueIdentifier; } }

        public int ParseInt(string key)
        {
            return int.Parse(this[key].ToString());
        }
        public float ParseFloat(string key)
        {
            return float.Parse(this[key].ToString());
        }

        public string ParseString(string key)
        {
            return this[key].ToString();
        }

        public SpreadSheetObject(string sheetName = null, int objectId = -1)
        {
            this.sheetName = sheetName ?? SpreadSheetSetting.Instance.DefalutSheetName;
            this.objectId = objectId;
        }

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
