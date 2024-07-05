using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ChatMessage : MonoBehaviour
{
    public static ChatMessage instance;
    [SerializeField] TextMeshProUGUI _chatText;
    [SerializeField] RectTransform _chatContent;
    [SerializeField] ScrollRect _chatScrollRect;
    
    public void SetText(string msg, string fromWho)
    {
        _chatText.text += _chatText.text == "" ? msg : "\n" + msg;

        Fit(_chatText.GetComponent<RectTransform>());
        Fit(_chatContent);
        Invoke("ScrollDelay", 0.03f);
    }

    void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

    void ScrollDelay() => _chatScrollRect.verticalScrollbar.value = 0;
}