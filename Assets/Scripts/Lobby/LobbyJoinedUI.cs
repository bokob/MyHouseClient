using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LobbyJoinedUI : MonoBehaviour
{
    public GameObject readyButton;

    public GameObject unreadyButton;

    public TMP_Text lobbyNameText;
    public TMP_Text lobbyCodeText;

    public GameObject leaveLobbyButton;

    private void Awake()
    {
        readyButton.SetActive(true);
    }

    private void Start()
    {
        Lobby lobby = LobbyManager.instance.GetJoinedLobby();
        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        //NetworkGameManager.instance.ChangePlayerSkin(Random.Range(0, 111));
    }
    public void ReadyPressed()
    {
        readyButton.SetActive(false);
        unreadyButton.SetActive(true);

        leaveLobbyButton.SetActive(false);
    }

    public void UnReadyPressed() 
    {
        unreadyButton.SetActive(false);
        readyButton.SetActive(true);

        leaveLobbyButton.SetActive(true);

    }

    public void LeaveLobbyPressed()
    {
        LobbyManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("LoadingScene");
    }
}
