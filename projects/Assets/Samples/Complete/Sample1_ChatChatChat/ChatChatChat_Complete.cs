using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GSSA;
using UnityEngine;
using UnityEngine.UI;

public class ChatChatChat_Complete : MonoBehaviour
{
    [SerializeField] private Text logText;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField messageInputField;

    private List<string> chatLogList = new List<string>();
    private long lastGetTime;

    void Start ()
	{
        messageInputField.onEndEdit.AddListener(SendChatMessage);
	    StartCoroutine(GetChatLogIterator());
	}

    private void SendChatMessage(string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        var so = new SpreadSheetObject("Chat");
        so["name"] = nameInputField.text ?? "名無しさん";
        so["message"] = s;
        so.SaveAsync();
        messageInputField.text = "";
    }

    private IEnumerator GetChatLogIterator()
    {
        while (true)
        {
            var query = new SpreadSheetQuery("Chat");
            query.OrderByDescending("createTime").Where("createTime",">",lastGetTime).Limit(20);
            yield return query.FindAsync();

            if (query.Count > 0)
            {
                foreach (var so in query.Result.Reverse())
                {
                    chatLogList.Insert(0,so["name"] + ">" + so["message"]);
                    if (chatLogList.Count > 17) chatLogList.Remove(chatLogList.Last());
                }
                logText.text = string.Join("\n", chatLogList.ToArray());
                lastGetTime = (long)query.Result.First()["createTime"];
            }

            yield return new WaitForSeconds(5.0f);
        }
    }
}
