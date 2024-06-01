using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI playerCountText;

    NetworkVariable<int> playerNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    
    private void Update()
    {
        playerCountText.text = $"{playerNum.Value.ToString()}";
        if(!IsServer) return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count; 
    }
}
