using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;
    public TextMeshProUGUI chatText;
    public RectTransform chatContent;
    public TMP_InputField sendInput;
    public ScrollRect chatScrollRect;

    void Start()
    {
        instance = this;
    }
    
    public void SendMsg(string msg)
    {
        chatText.text += chatText.text == "" ? msg : "\n" + msg;
        sendInput.text = "";
        sendInput.ActivateInputField();

        Fit(chatText.GetComponent<RectTransform>());
        Fit(chatContent);
        Invoke("ScrollDelay", 0.03f);
    }

    void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

    void ScrollDelay() => chatScrollRect.verticalScrollbar.value = 0;
}