using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class ReadyUp : NetworkBehaviour
{
    LobbyJoinedUI ui;
    Dictionary<ulong, bool> playerReadyDictionary; // 각 클라이언트의 준비 상태를 저장하는 딕셔너리

    public bool isLocalPlayerReady = false;        // 로컬 플레이어(자기 자신)의 준비 상태를 나타낸다. 
    // Start is called before the first frame update
    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        ui = GetComponent<LobbyJoinedUI>();
    }
    public void SetLocalPlayerReady()
    {
        isLocalPlayerReady = true;
        SetPlayerReadyServerRpc(NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId));
        //Debug.Log(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerReadyServerRpc(int _index, ServerRpcParams rpcParams = default) // 서버에서 호출되는 RPC 메서드
    {
        // ServerRpcParams는 RPC를 호출한 클라이언트로부터 전달된 정보를 담고 있는 속성

        playerReadyDictionary[rpcParams.Receive.SenderClientId] = true; // Rpc를 호출한 클라이언트의 준비 상태를 true로 설정

        SetPlayerReadyDisplayClientRpc(_index); // 다른 클라이언트에게 보여주기 위해 서버에서 실행한다. 그러면 다른 클라이언트에서 이 RPC 호출한다.

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //this player not ready
                allClientsReady = false;
                break;
            }
        }
        Debug.Log("All clients ready: " + allClientsReady);
        if (allClientsReady == true) // 모든 클라이언트가 준비 상태면, 게임씬 로드
        {
            NetworkGameManager.instance.LoadGameScene();
        }
    }

    [ClientRpc]
    void SetPlayerReadyDisplayClientRpc(int _index)
    {
        //GameManager.instance.GetPlayerDataFromIndex(_index);
        LobbyPlayer[] _players = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer player in _players)
        {
            if (player.index == _index)
            {
                player.usernameText.color = Color.green;
                break;
            }
        }
    }

    public void SetLocalPlayerUnready()
    {
        isLocalPlayerReady = false;
        SetPlayerUnreadyServerRpc(NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerUnreadyServerRpc(int _index, ServerRpcParams rpcParams = default) // 서버에서 실행하는 RPC 메서드
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = false;
        SetPlayerUnreadyDisplayClientRpc(_index); // 다른 클라이언트도 반영할 수 있도록 클라이언트 RPC 호출
    }
    [ClientRpc]
    void SetPlayerUnreadyDisplayClientRpc(int _index) // 특정 플레이어의 준비 취소 상태를 UI에 표시
    {
        LobbyPlayer[] _players = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer player in _players)
        {
            if (player.index == _index)
            {
                player.usernameText.color = Color.white;
                break;
            }
        }
    }
}
