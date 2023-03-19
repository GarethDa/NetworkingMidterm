using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChatBoxBehaviour : MonoBehaviour
{
    [SerializeField] ContentSizeFitter contentSizeFitter;
    [SerializeField] TMP_Text showHideText;
    [SerializeField] Transform messageParentPanel;
    [SerializeField] GameObject newMessagePrefab;
    [SerializeField] Client userClient;

    Queue<string> messageQueue = new Queue<string>();

    bool showingChat = false;

    string message = "";

    private void Start()
    {
        ToggleChat();
    }

    private void Update()
    {
        if (messageQueue.Count > 0)
        {
            SetMessage(messageQueue.Dequeue());

            ShowReceivedMessage();
        }
    }

    public void ToggleChat ()
    {
        showingChat = !showingChat;

        //Show the chat
        if (showingChat)
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            showHideText.text = "Hide Chat";
        }

        //Hide the chat
        else
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            showHideText.text = "Show Chat";
        }
    }

    public void SetMessage(string message)
    {
        this.message = message;
    }

    //For showing messages that this client has sent
    public void ShowMessage()
    {
        if (message != "")
        {
            //Create a new message object, place it into the canvas
            GameObject clone = Instantiate(newMessagePrefab);
            clone.transform.SetParent(messageParentPanel);
            clone.transform.SetSiblingIndex(messageParentPanel.childCount - 2);
            clone.GetComponent<MessageBehaviour>().ShowMessage(message);

            userClient.SendMsg("msg: " + message + userClient.GetPlayerNum());
        }

        else
            Debug.Log("Message is blank");
    }

    //For showing messages this client has received
    public void ShowReceivedMessage()
    {
        if (message != "")
        {
            //Create a new message object, place it into the canvas
            GameObject clone = Instantiate(newMessagePrefab);
            clone.transform.SetParent(messageParentPanel);
            clone.transform.SetSiblingIndex(messageParentPanel.childCount - 2);
            clone.GetComponent<MessageBehaviour>().ShowMessage(message);
        }

        else
            Debug.Log("Message is blank");
    }

    //For queueing up messages to be displayed
    //This is since we cannot instantiate within a callback (must be on the main thread),
    //so instead we queue messages and instantiate them afterwards within Update()
    public void QueueMessage(string msg)
    {
        messageQueue.Enqueue(msg);
    }
}
