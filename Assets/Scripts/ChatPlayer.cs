using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class ChatPlayer : NetworkBehaviour
{
    TMP_InputField sendInput;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            base.OnNetworkSpawn();
            AddChatServerRpc("Player " + OwnerClientId + " joined the chat.");
            sendInput = ChatManager.instance.sendInput;
            sendInput.onSubmit.AddListener(SendMessageFromUI);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        AddChatServerRpc("Player " + OwnerClientId + " left the chat.");
    }

    public void SendMessageFromUI(string msg)
    {
        sendInput.text = "";
        AddChatServerRpc(msg);
    }

    [ServerRpc(RequireOwnership = false)]
    void AddChatServerRpc(string v)
    {
        AddChatClientRpc(v);
    }

    [ClientRpc]
    void AddChatClientRpc(string v)
    {
        ChatManager.instance.SendMsg(v);

    }
}
