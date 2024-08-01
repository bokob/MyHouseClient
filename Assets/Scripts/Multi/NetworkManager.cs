using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager _instance;

    public string _roomNameToJoin = "test"; // 참가할 방 이름 


    [Header("DisconnectPanel")]
    public TMP_InputField _nickNameInput; // 닉네임 이름

    [Header("LobbyPanel")]
    public GameObject _lobbyPanel;
    public TextMeshPro _lobbyInfoText;    // 로비 상태 보여줄 UI(몇 명이 로비에 있는지, 몇 명이 MyHouse에 접속했는지)
    public GameObject _roomListUI;        // 방 목록(방들 표시되는 곳을 가리킴)
    public GameObject _roomListItemUI;    // 방 (방 목록에 표시되는 게임 방을 가리킴)
    List<RoomInfo> _cacheRoomList = new List<RoomInfo>(); // 방 정보 모아둘 리스트


    [Header("RoomPanel")]
    public GameObject _roomPanel;
    public TextMeshPro _roomName;       // 방 제목
    public TextMeshPro _roomInfoText;   // 방 정보 보여줄 UI
    public GameObject[] _roomPlayers;     // 방에 접속한 플레이어 나타내는 UI
    List<Player> _cachePlayerList = new List<Player>(); // 방 정보 모아둘 리스트

    // 채팅 관련
    public GameObject _chatListUI;
    public GameObject _chatListItemUI;
    public TMP_InputField _chatInput; // 채팅 입력창

    [Header("ETC")]
    public TextMeshPro StatusText;
    public PhotonView PV;

    [Header("알림 메시지 관련")]
    public GameObject connectionResponseUI;             // '연결중' 나타내는 UI
    public TMP_Text messsageText;                       // 연결 실패 문구
    public GameObject connectionResponseCloseButton;    // 연결 실패 했을 때 닫는 버튼

    #region 서버연결
    void Awake()
    {
        if (_instance == null)
            _instance = this;

        DontDestroyOnLoad(_instance);
    }
    void Update()
    {
        _lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "Lobby / " + PhotonNetwork.CountOfPlayers + "Connected"; // 방 정보 보여주기
        
        // 엔터 칠 때, 방에 있고 화이트 스페이스가 아니여야 함
        if(PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(_chatInput.text) && Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        }
        
    }

    // 포톤 서버에 연결
    public void Connect()
    {
        Debug.Log("게임 서버에 접속합니다.");
        PhotonNetwork.ConnectUsingSettings();
    }

    // 서버에 연결하고, 콜백으로 호출됨 -> 로비에 입장하게 됨
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    // 로비 입장했을 때 콜백
    public override void OnJoinedLobby()
    {
        //_lobbyPanel.SetActive(true);
        //_roomPanel.SetActive(false);

        if(string.IsNullOrWhiteSpace(_nickNameInput.text))  // 아무것도 입력되어 있지 않은 경우
        {
            _nickNameInput.text = UnityEngine.Random.Range(100, 1000).ToString(); // 숫자를 랜덤으로 부여
        }

        PhotonNetwork.LocalPlayer.NickName = _nickNameInput.text;
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 님이 로비에 접속했습니다.");
        _cacheRoomList.Clear();
    }

    // 포톤 서버 연결 끊기
    public void Disconnect()
    {
        Debug.Log("게임 종료(서버와도 연결을 종료)합니다.");
        PhotonNetwork.Disconnect();
    }

    // 서버 연결 끊으면, 콜백으로 호출됨
    public override void OnDisconnected(DisconnectCause cause) 
    {
        //_lobbyPanel.SetActive(false);
        //_roomPanel.SetActive(false);
    }
    #endregion

    #region 방

    // 방 만들기
    public void CreateRoom()
    {
        // 방 생성
        PhotonNetwork.CreateRoom(_roomName.text = "Room " + UnityEngine.Random.Range(100, 1000).ToString(), new RoomOptions { // 방 옵션
            MaxPlayers = 4,     // 최대 인원 수
            EmptyRoomTtl = 0    // 방이 비어 있을 때 즉시 삭제  
        });
        
        // connectionResponseUI.SetActive(true); // 추후에 예외처리로 메시지 받아올 예정
    }

    // 빠르게 랜덤으로 접속
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // 방 떠나기
    public void LeaveRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " 을(를) 퇴장합니다.");

        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom() // 1. 방을 만들고 들어갈 때 2. 바로 방으로 들어갈 때
    {
        Debug.Log( PhotonNetwork.CurrentRoom.Name + " 에 참가합니다.");
        //RoomPanel.SetActive(true);
        _cachePlayerList.Clear();
        UpdateUserInRoomUI();
    }

    //// 로컬 플레이어가 방을 나갔을 때, 호출
    //public override void OnLeftRoom()
    //{
    //    base.OnLeftRoom();

    //    // 메인 메뉴로 돌아가게 하기
    //}

    // 방 만들기 실패했을 때 콜백
    public override void OnCreateRoomFailed(short returnCode, string message) 
    { 
        _roomName.text = "Room"; 
        CreateRoom(); 
    }

    // 랜덤 접속 실패했을 때 콜백
    public override void OnJoinRandomFailed(short returnCode, string message) 
    { 
        _roomName.text = ""; 
        CreateRoom(); 
    }

    // 방에 입장했을 때, 안에 있던 모두에게 전달
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");


        UpdateUserInRoomUI();
    }

    // 방을 떠날 때, 안에 있던 모두에게 전달
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        UpdateUserInRoomUI();
    }

    void UpdateUserInRoomUI() // 방 인원에 맞게 UI 업데이트
    {
        Player[] playerList = PhotonNetwork.PlayerList; // 현재 방에 있는 모든 플레이어 목록 가져오기
        int playerCount = playerList.Length;

        for (int i = 0; i < playerCount; i++)
        {
            Player player = playerList[i];

            if (!_cachePlayerList.Contains(player))
            {
                _cachePlayerList.Add(player);
                Debug.Log("Player added: " + player.NickName);
            }
            else
            {
                // 필요한 경우 기존 플레이어 정보 업데이트
                int index = _cachePlayerList.IndexOf(player);
                _cachePlayerList[index] = player;
                Debug.Log("Player updated: " + player.NickName);
            }
        }

        // 리스트에 있는 플레이어 중 방에 없는 플레이어 제거
        for (int i = _cachePlayerList.Count - 1; i >= 0; i--)
        {
            if (System.Array.IndexOf(playerList, _cachePlayerList[i]) == -1)
            {
                Debug.Log("Player removed: " + _cachePlayerList[i].NickName);
                _cachePlayerList.RemoveAt(i);
            }
        }
        
        // 플레이어 정보 UI에 업데이트
        for (int i=0; i< 4; i++)
        {
            if (i < _cachePlayerList.Count)
            {
                _roomPlayers[i].SetActive(true);
                _roomPlayers[i].transform.GetChild(0).GetComponent<TextMeshPro>().text = PhotonNetwork.PlayerList[i].NickName;
                Debug.Log(_cachePlayerList[i].NickName + "의 UI 활성화");
            }
            else
            {
                _roomPlayers[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 방 버튼 눌러서 입장
    /// </summary>
    public void JoinRoomButtonPressed() 
    {
        Debug.Log("연결중...");

        PhotonNetwork.JoinOrCreateRoom(_roomNameToJoin, new RoomOptions
        { // 방 옵션
            MaxPlayers = 4,     // 최대 인원 수
            EmptyRoomTtl = 0    // 방이 비어 있을 때 즉시 삭제  
        }, null);

        /*
         연결중이라는 문구 활성화시킬 예정 
        */

        //nameUI.SetActive(false);
        //connectingUI.SetActive(true);
    }
    #endregion

    #region 방리스트 갱신
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 방 목록이 바귀면 호출되는 콜백
    {
        int roomCount = roomList.Count;

        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!_cacheRoomList.Contains(roomList[i])) _cacheRoomList.Add(roomList[i]);
                else _cacheRoomList[_cacheRoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (_cacheRoomList.IndexOf(roomList[i]) != -1) _cacheRoomList.RemoveAt(_cacheRoomList.IndexOf(roomList[i]));
        }
        UpdateRoomListUI();
    }

    void UpdateRoomListUI() // 방 목록 UI 업데이트
    {
        // 기존 방 목록 UI 삭제하기
        foreach (Transform child in _roomListUI.transform)
        {
            Destroy(child.gameObject);  // 기존 로비 목록 UI 모두 삭제
        }

        // 방 UI 생성
        foreach (RoomInfo room in _cacheRoomList)
        {
            GameObject _room = Instantiate(_roomListItemUI, _roomListUI.transform);   // 방 템플릿 복제해서 실제 방 UI 생성
            _room.SetActive(true);
            _room.GetComponent<RoomListItemUI>().SetRoom(room); // 각 로비 UI에 실제 로비 데이터 설정
        }
    }
    #endregion

    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + _chatInput.text); // 방에 있는 모두에게 명령
        _chatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        GameObject chatManager = Instantiate(_chatListItemUI, _chatListUI.transform);
        chatManager.GetComponent<ChatMessage>().SetText(msg);
    }
    #endregion
}
