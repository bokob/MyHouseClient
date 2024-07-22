using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnNewClientConnected;
        }
    }
    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // server
        NetworkGameManager.instance.SpawnPlayers();

        if (IsServer)
        {
            // 
        }
    }

    private void OnNewClientConnected(ulong clientId)
    {
        Debug.Log(clientId + "(이)가 입장했습니다.");

        NetworkGameManager.instance.SpawnPlayer(clientId);
        if (IsServer)
        {
            
        }
    }
}
