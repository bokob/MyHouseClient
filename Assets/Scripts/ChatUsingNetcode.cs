using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ChatUsingNetcode : NetworkBehaviour
{
    public static ChatUsingNetcode singleton;
    [SerializeField] TMP_InputField _sendInput;

    string chatUserName;

    void Start()
    {
        chatUserName = NetworkGameManager.instance.GetUsername();
    }

    void Awake()
    {
        ChatUsingNetcode.singleton = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(_sendInput.text, chatUserName);
            _sendInput.text = "";
        }
    }

    void SendChatMessage(string message, string username)
    {
        AddChatServerRpc(message, username);
    }

    [ServerRpc(RequireOwnership = false)]
    void AddChatServerRpc(string str, string name)
    {
        AddChatClientRpc(str, name);
    }

    [ClientRpc]
    void AddChatClientRpc(string msg, string nickname)
    {
        ChatMessage.instance.SetText(msg, nickname);

    }
}
