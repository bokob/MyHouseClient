using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 로비 참여 및 나가기 관련 클래스
/// </summary>
public class LobbyJoinedUI : MonoBehaviour
{
    public GameObject readyButton; // 준비 버튼
    public GameObject unreadyButton; // 준비해제 버튼

    public TMP_Text lobbyNameText; // 로비 이름 표시되는 곳
    public TMP_Text lobbyCodeText; // 로비 코드 표시되는 곳

    public GameObject leaveLobbyButton; // 로비 떠나기 버튼

    private void Awake()
    {
        readyButton.SetActive(true); // 준비 버튼 활성화
    }

    private void Start()
    {
        Lobby lobby = LobbyManager.instance.GetJoinedLobby();   // 현재 참여중인 로비 가져오기
        lobbyNameText.text = lobby.Name;                        // 로비 이름 표시
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;  // 로비 코드 표시
    }

    public void ReadyPressed() // 준비 버튼 눌렀을 때 호출되는 메서드
    {
        readyButton.SetActive(false);
        unreadyButton.SetActive(true);

        leaveLobbyButton.SetActive(false);
    }

    public void UnReadyPressed() // 준비 해제 버튼 눌렀을 때 호출되는 메서드
    {
        unreadyButton.SetActive(false);
        readyButton.SetActive(true);

        leaveLobbyButton.SetActive(true);

    }

    public void LeaveLobbyPressed() // 로비 떠나기 버튼 눌렀을 때 호출되는 메서드
    {
        LobbyManager.instance.LeaveLobby();     // 로비 떠나기
        // NetworkManager.Singleton.ConnectionApprovalCallback = null; // 호스트가 방 만들고 나간 다음, 다시 방을 만들었을 때 오류 안나게 Approval 을 다시 null로 해줌 (혹시 모르니 지우지 말기)
        NetworkManager.Singleton.Shutdown();    // 네트워크 세션 종료
        DestroyMultiManagers();
        SceneManager.LoadScene("LoadingScene");
    }

    public void DestroyMultiManagers()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);
        if (NetworkGameManager.instance != null)
            Destroy(NetworkGameManager.instance.gameObject);
        if (LobbyManager.instance != null)
            Destroy(LobbyManager.instance.gameObject);
    }
}
