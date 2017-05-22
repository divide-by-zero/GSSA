using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GSSA;
using UnityEngine;
using UnityEngine.UI;

public class ChatChatChat : MonoBehaviour
{
    [SerializeField] private Text logText;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField messageInputField;

    void Start ()
    {
        messageInputField.onEndEdit.AddListener(SendChatMessage);
        StartCoroutine(GetChatLogIterator());
    }

    private void SendChatMessage(string s)
    {

    }

    private IEnumerator GetChatLogIterator()
    {
        while (true)
        {

            yield return new WaitForSeconds(5.0f);
        }
    }
}