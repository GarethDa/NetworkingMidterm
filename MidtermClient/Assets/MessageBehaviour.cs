using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageBehaviour : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public void ShowMessage (string message)
    {
        text.text = message;
    }

    public void HideMessage()
    {
        Destroy(gameObject);
    }
}
