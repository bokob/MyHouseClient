using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public int index;

    public TMP_Text usernameText;

    private void Start()
    {
        NetworkGameManager.onPlayerDataListChanged += UpdatePlayer;
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        //Debug.Log("Update");
        if (NetworkGameManager.instance.IsPlayerIndexConnected(index))
        {
            Show();

            PlayerData data = NetworkGameManager.instance.GetPlayerDataFromIndex(index);
            usernameText.text = data.username.ToString();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        gameObject.SetActive(true);

    }
    void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void OnDestroy()
    {
        NetworkGameManager.onPlayerDataListChanged -= UpdatePlayer;
    }
}
