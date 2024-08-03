using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager _instance;

    public string _roomNameToJoin = "test"; // 참가할 방 이름 


    [Header("DisconnectPanel")]
    public TMP_InputField _nickNameInput; // 닉네임 이름

    [Header("LobbyPanel")]
    public GameObject _lobbyPanel;
    public TextMeshPro _welcomeText;
    public TextMeshPro _lobbyInfoText;
    public GameObject _roomListUI;
    public GameObject _roomListItemUI;    // 방 (방 목록에 표시되는 게임 방)
    List<RoomInfo> cacheRoomList = new List<RoomInfo>(); // 방 정보 모아둘 리스트


    [Header("RoomPanel")]
    public GameObject _roomPanel;
    public TextMeshPro _roomName; // 방 제목
    public TextMeshPro ListText;
    public TextMeshPro _roomInfoText;

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
        PhotonNetwork.LocalPlayer.NickName = _nickNameInput.text;
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 님 로비에 접속했습니다.");
        cacheRoomList.Clear();
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
        PhotonNetwork.CreateRoom(_roomName.text = "Room" + UnityEngine.Random.Range(100, 1000).ToString(), new RoomOptions { // 방 옵션
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
        //RoomRenewal();
        //ChatInput.text = "";
        //for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
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
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    // 방을 떠날 때, 안에 있던 모두에게 전달
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("여러분 " + PhotonNetwork.CurrentRoom.Name + "님이 퇴장한다고 합니다.");
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {        
        for(int i=0; i<PhotonNetwork.PlayerList.Length;i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
        Debug.Log("이상");
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
                if (!cacheRoomList.Contains(roomList[i])) cacheRoomList.Add(roomList[i]);
                else cacheRoomList[cacheRoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (cacheRoomList.IndexOf(roomList[i]) != -1) cacheRoomList.RemoveAt(cacheRoomList.IndexOf(roomList[i]));
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
        foreach (RoomInfo room in cacheRoomList)
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

    //[PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    //void ChatRPC(string msg)
    //{
    //    bool isInput = false;
    //    for (int i = 0; i < ChatText.Length; i++)
    //        if (ChatText[i].text == "")
    //        {
    //            isInput = true;
    //            ChatText[i].text = msg;
    //            break;
    //        }
    //    if (!isInput) // 꽉차면 한칸씩 위로 올림
    //    {
    //        for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
    //        ChatText[ChatText.Length - 1].text = msg;
    //    }
    //}

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        GameObject chatManager = Instantiate(_chatListItemUI, _chatListUI.transform);
        chatManager.GetComponent<ChatMessage>().SetText(msg);
    }
    #endregion
}
