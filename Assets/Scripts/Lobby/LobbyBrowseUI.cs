using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBrowseUI : MonoBehaviour
{
    public TMP_InputField usernameInput;    // 사용자 이름 입력창
    public TMP_InputField lobbyNameInput;   // 로비 이름 입력
    public TMP_InputField joinCodeInput;    // 참여 코드 입력창
    public GameObject codeInput;            
    public Toggle isPrivate;                // 비밀 방 만들건지 여부
    public GameObject lobbyContainer;       // 로비 목록들 표시할 곳
    public GameObject lobbyListTemplate;    // 로비 (로비 목록에 표시되는 게임 방)

    // 알림 메시지 용 코드
    public GameObject connectionResponseUI;             // '연결중' 나타내는 UI
    public TMP_Text messsageText;                       // 연결 실패 문구
    public GameObject connectionResponseCloseButton;    // 연결 실패 했을 때 닫는 버튼

    public static LobbyBrowseUI instance;
    private void Awake() 
    {
        instance = this;    // 싱글톤 만들기

        lobbyListTemplate.SetActive(false); // 로비 템플릿 비활성화, 실제 로비 생성할 때 복제하여 사용
        lobbyContainer.SetActive(true);     // 로비 컨테이너 생성, 로비들이 표시됨
    }
    private void Start() 
    {
        SoundManager.instance.PlayBGM(0);
        usernameInput.text = NetworkGameManager.instance.GetUsername(); // 사용자명 입력 필드 초기값 설정
        usernameInput.onValueChanged.AddListener((string newText) =>
        {
            NetworkGameManager.instance.SetUsername(newText);           // 사용자 명 변경 시 NetworkGameManager에 새로운 사용자명 설정
        });
        LobbyManager.instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;   // 이벤트 등록, 로비 목록이 변경될 때마다 호출
    }
    private void GameLobby_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }
    public void CreateLobbyPressed()    // 로비 생성 버튼 눌렀을 때 호출되는 메서드
    {
        //NetworkManager.Singleton.StartHost();
        connectionResponseUI.SetActive(true);
        string _lobby = "Lobby";
        messsageText.text = "Connecting...";

        if (lobbyNameInput.text == "" || lobbyNameInput.text == null)
        {
            _lobby = ("Lobby " + UnityEngine.Random.Range(100, 1000).ToString());
        }
        else
        {
            _lobby = lobbyNameInput.text;
        }
        LobbyManager.instance.CreateLobby(_lobby, isPrivate.isOn);
    }
    public void QuickJoinPressed()  // 빠른 참여 눌렀을 때 호출되는 메서드
    {
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";
        LobbyManager.instance.QuickJoin();
        //NetworkManager.Singleton.StartClient();
    }

    public void JoinCodePressed()   // join code 입력하는 곳에 코드 입력하고 Enter 누르면 호출되는 메서드
    {
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";
        LobbyManager.instance.JoinByCode(joinCodeInput.text);
        codeInput.SetActive(false);
    }
    
    public void JoinLobbyById(string _lobbyId) // 로비 눌렀을 때 호출되는 메서드
    {
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";
        LobbyManager.instance.JoinByID(_lobbyId);
    }

    public void UpdateLobbyList(List<Lobby> lobbyList)  // 로비 목록이 변경될 때 호출되어 UI 업데이트
    {
        foreach (Transform child in lobbyContainer.transform)
        {
            if (child == lobbyListTemplate) continue;
            Destroy(child.gameObject);  // 기존 로비 목록 UI 모두 삭제
        }
        foreach (Lobby lobby in lobbyList)
        {
            GameObject _lobby = Instantiate(lobbyListTemplate, lobbyContainer.transform);   // 로비 목록 템플릿 복제해서 실제 로비 UI 생성
            _lobby.SetActive(true);
            _lobby.GetComponent<LobbyListItemUI>().SetLobby(lobby); // 각 로비 UI에 실제 로비 데이터 설정
        }
    }

    public void ConnectionFailed() // 로비 연결 실패시에 문구 나오게 하는 메서드
    {
        messsageText.text = NetworkManager.Singleton.DisconnectReason.ToString();
        connectionResponseCloseButton.SetActive(true);
    }
    public void LobbyConnectError(string reason) // 로비 연결 오류 메시지 나오게 하는 메서드
    {
        messsageText.text = reason;
        connectionResponseCloseButton.SetActive(true);
    }

    public void CloseConnectionResponseUI() // 오류 났을 때 닫기 버튼 누르면 호출되는 메서드
    {
        connectionResponseUI.SetActive(false);
        connectionResponseCloseButton.SetActive(false);
    }

    public void CodeInputActive() // 'Join To Code' 누르면 호출되는 메서드, 코드 입력하는 곳이 나온다.
    {
        codeInput.SetActive(true);
        joinCodeInput.ActivateInputField();
    }

    /// <summary>
    /// 오브젝트가 파괴될 때, 이벤트 등록 해제
    /// </summary>
    private void OnDestroy()
    {
        LobbyManager.instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged;
    }
}
