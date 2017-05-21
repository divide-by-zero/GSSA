using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSSA
{
    public class SpreadSheetSetting : MonoBehaviour
    {
        [SerializeField] private string _spreadSheetUrl;
        [SerializeField] private bool _isDebugLogOutput;
        [SerializeField] private string _defaultSheetName;

        public string SpreadSheetUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_spreadSheetUrl)) throw new Exception("SpreadSheetSettingが正しく初期化されていません");
                return _spreadSheetUrl;
            }
        }

        public string DefalutSheetName
        {
            get { return _defaultSheetName; }
        }

        public bool IsDebugLogOutput
        {
            get { return _isDebugLogOutput; }
        }

        public string UniqueID
        {
            get
            {
                if (PlayerPrefs.HasKey("guid") == false)
                {
                    PlayerPrefs.SetString("guid",Guid.NewGuid().ToString());
                }
                return PlayerPrefs.GetString("guid");
            }
        }

        private static SpreadSheetSetting sinstance;
        public static SpreadSheetSetting Instance
        {
            get
            {
                if (sinstance == null)
                {
                    sinstance = FindObjectOfType<SpreadSheetSetting>();
                    if (sinstance == null)
                    {
                        var obj = new GameObject(typeof(SpreadSheetSetting).Name);
                        sinstance = obj.AddComponent<SpreadSheetSetting>();
                    }
                }
                return sinstance;
            }
        }

        void Awake()
        {
            StartCoroutine(MainThreadDispatchIterator());

            if (this == Instance)
            {
                DontDestroyOnLoad(Instance);
                return;
            }
            Destroy(gameObject);
        }

        private IEnumerator MainThreadDispatchIterator()
        {
            while (true)
            {
                Func<IEnumerator> f = null;
                lock (_syncObject)
                {
                    if(_yieldFuncQueue.Any())f = _yieldFuncQueue.Dequeue();
                }
                if (f != null)yield return f();
                yield return null;
            }
        }

        private object _syncObject = new System.Object();
        private Queue<Func<IEnumerator>> _yieldFuncQueue = new Queue<Func<IEnumerator>>();

        public void Enqueue(Func<IEnumerator> f)
        {
            lock (_syncObject)
            {
                _yieldFuncQueue.Enqueue(f);
            }
        }
    }
}