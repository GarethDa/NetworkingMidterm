using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatBoxBehaviour : MonoBehaviour
{
    [SerializeField] ContentSizeFitter contentSizeFitter;
    [SerializeField] TMP_Text showHideText;
    [SerializeField] Transform messageParentPanel;
    [SerializeField] GameObject newMessagePrefab;


    bool showingChat = false;

    string message = "";

    private void Start()
    {
        ToggleChat();
    }

    public void ToggleChat ()
    {
        showingChat = !showingChat;

        if (showingChat)
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            showHideText.text = "Hide Chat";
        }

        else
        {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            showHideText.text = "Show Chat";
        }
    }

    public void SetMessage(string message)
    {
        Debug.Log(message);

        this.message = message;
    }

    public void ShowMessage()
    {
        if (message != "")
        {
            GameObject clone = (GameObject)Instantiate(newMessagePrefab);
            clone.transform.SetParent(messageParentPanel);
            clone.transform.SetSiblingIndex(messageParentPanel.childCount - 2);
            clone.GetComponent<MessageBehaviour>().ShowMessage(message);
        }
    }

}
