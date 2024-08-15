using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using SlimUI.ModernMenu;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager _instance;

    public const string _gameStartedPropKey = "IsGameStarted";
    public bool _isGameStarted;
    public string _nickName;
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
    public GameObject _startBtn;

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

    //[Header("게임 로직 관련")]
    //public List<GameObject> _playerPrefabListInRoom = new List<GameObject>();

    #region 서버연결
    void Awake()
    {
        if (_instance == null)
            _instance = this;

        DontDestroyOnLoad(_instance);
    }

    void Update()
    {
        if (!_isGameStarted)
        {
            _lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "Lobby / " + PhotonNetwork.CountOfPlayers + "Connected"; // 방 정보 보여주기

            // 엔터 칠 때, 방에 있고 화이트 스페이스가 아니여야 함
            if (PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(_chatInput.text) && Input.GetKeyDown(KeyCode.Return))
            {
                Send();
            }
        }
    }

    // 포톤 서버에 연결
    public void Connect()
    {
        // Debug.Log("게임 서버 접속");

        UIMenuManager._instance.responseMain.SetActive(true);


        if(PhotonNetwork.ConnectUsingSettings())
        {
            //Debug.Log("서버 접속 성공");
        }
        else
        {
            //Debug.Log("서버에 접속 실패");

            // 메인 화면에서 연결 실패 문구 활성화
            UIMenuManager._instance.responseMain.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Server Connection Failed";
        }
    }

    // 서버에 연결하고, 콜백으로 호출됨 -> 로비에 입장하게 됨
    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.JoinLobby())
        {
            // 메인 화면에서 연결중 문구 비활성화

            Debug.Log("로비 접속 성공");
            UIMenuManager._instance.responseMain.SetActive(false);
            UIMenuManager._instance.MainToLobbyCamPos();
        }
        else
        {
            Debug.Log("로비 접속 실패");
            UIMenuManager._instance.responseMain.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Access Lobby";
        }
    }

    // 로비 입장했을 때 콜백
    public override void OnJoinedLobby()
    {
        UIMenuManager._instance.responseRoom.SetActive(false);

        if (string.IsNullOrWhiteSpace(_nickNameInput.text))  // 아무것도 입력되어 있지 않은 경우
        {
            _nickName = UnityEngine.Random.Range(100, 1000).ToString(); // 숫자를 랜덤으로 부여
            _nickNameInput.text = _nickName;
        }
        _nickName = _nickNameInput.text;
        PhotonNetwork.LocalPlayer.NickName = _nickName;
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 님이 로비에 접속했습니다.");
        _cacheRoomList.Clear();
    }

    // 포톤 서버 연결 끊기
    public void Disconnect()
    {
        Debug.Log("게임 서버 연결 종료");
        UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Discconecting to Server...";
        PhotonNetwork.Disconnect();
    }

    // 서버 연결 끊으면, 콜백으로 호출됨
    public override void OnDisconnected(DisconnectCause cause) 
    {
        //Debug.Log("연결 종료 이유: " + cause.ToString());
        UIMenuManager._instance.responseLobby.SetActive(false);
        UIMenuManager._instance.LobbyToMainCamPos();
    }
    #endregion

    #region 방

    // 방 만들기
    public void CreateRoom()
    {
        UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Creating a room...";
        UIMenuManager._instance.responseLobby.SetActive(true);

        // 방 생성
        if (PhotonNetwork.CreateRoom(_roomName.text = "Room " + UnityEngine.Random.Range(100, 1000).ToString(), new RoomOptions { 
            // 방 옵션
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 4,     // 최대 인원 수
            EmptyRoomTtl = 0    // 방이 비어 있을 때 즉시 삭제  
        }))
        {
            Debug.Log("방 생성 성공");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            Debug.Log("방 생성 실패");
            UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Create Room";
        }
    }

    // 빠르게 랜덤으로 접속
    public void JoinRandomRoom()
    {
        UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Quick Start...";
        if (PhotonNetwork.JoinRandomRoom())
        {
            //Debug.Log("랜덤 참여 성공");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            //Debug.Log("랜덤 참여 실패");
            UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Quick Start";
        }
    }

    // 방 떠나기
    public void LeaveRoom()
    {

        //Debug.Log(PhotonNetwork.CurrentRoom.Name + " 을(를) 퇴장합니다.");

        UIMenuManager._instance.responseRoom.SetActive(true);
        UIMenuManager._instance.responseRoom.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Leave Room...";
        
        if (PhotonNetwork.LeaveRoom())
        {
            //Debug.Log("방 떠나기 성공");
            _startBtn.SetActive(false);
            UIMenuManager._instance.RoomToLobbyCamPos();
        }
        else
        {
            UIMenuManager._instance.responseRoom.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Leave Room";
            //Debug.Log("방 떠나기 실패");
        }
    }

    // 1. 방을 만들고 들어갈 때 2. 바로 방으로 들어갈 때 콜백
    public override void OnJoinedRoom()
    {
        IsGameStarted(); // 게임 진행중인지 확인
        if (_isGameStarted)
        {
            PhotonNetwork.LoadLevel("MultiPlayScene");
            return; // 게임 중이면 방 로직 실행 안 함
        }
        UIMenuManager._instance.responseLobby.SetActive(false);
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " 에 참가합니다.");
        //RoomPanel.SetActive(true);
        _cachePlayerList.Clear();

        if (PhotonNetwork.IsMasterClient)
            _startBtn.SetActive(true);

        UpdateUserInRoomUI();
    }

    // 방 만들기 실패했을 때 콜백
    public override void OnCreateRoomFailed(short returnCode, string message) 
    { 
        Debug.Log(returnCode.ToString() + message);

        _roomName.text = "Room"; 
        CreateRoom(); 
    }

    // 방 랜덤 참여 실패했을 때 콜백
    public override void OnJoinRandomFailed(short returnCode, string message) 
    {
        Debug.Log(returnCode.ToString() + message);

        _roomName.text = ""; 
        CreateRoom(); 
    }

    // 방에 입장했을 때, 안에 있던 모두에게 전달
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (_isGameStarted) return; // 게임 중이면 방 로직 실행 안 함

        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
        UpdateUserInRoomUI();
    }

    // 방을 떠날 때, 안에 있던 모두에게 전달
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_isGameStarted)
        {
            UpdateCachePlayerList(); // 방 안에 있는 플레이어 리스트 업데이트, 누군가 마스터 클라이언트로 지정되면 집주인으로 바꿔야 하기 때문

            //Debug.Log(otherPlayer.NickName + "(이)가 나갔습니다.");

            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObject in playerObjects)
            {
                PhotonView photonView = playerObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.Owner == otherPlayer)
                {
                    photonView.RPC("SmokeEffect", RpcTarget.All, playerObject.transform.position);
                    Destroy(playerObject.transform.parent.gameObject); // 오브젝트 제거
                    break;
                }
            }

            return; // 게임 중이면 방 로직 실행 안 함
        }

        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        UpdateUserInRoomUI();
    }

    void UpdateUserInRoomUI() // 방안의 플레이어에 맞게 UI 업데이트
    {

        if (PhotonNetwork.IsMasterClient) // 시작 버튼 마스터 클라이언트면 활성화
        {
            _startBtn.SetActive(true);
        }

        UpdateCachePlayerList();

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

    // 방에 접속한 플레이어 리스트 관리
    void UpdateCachePlayerList()
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
    }

    /// <summary>
    /// 방 버튼 눌러서 입장
    /// </summary>
    public void JoinRoom() 
    {
        //Debug.Log("방에 연결중...");

        if(PhotonNetwork.JoinOrCreateRoom(_roomNameToJoin, new RoomOptions
        { // 방 옵션
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 4,     // 최대 인원 수
            EmptyRoomTtl = 0    // 방이 비어 있을 때 즉시 삭제  
        }, null))
        {
            //Debug.Log("방 참여 성공");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            //Debug.Log("방 참여 실패");
        }

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


    #region 게임 로직

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("로드된 씬: " + scene.name);


        // 플레이어 소환

        if(scene.name == "MultiPlayScene")
            GameManager._instance.SapwnPlayer();
}

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트의 경우
        {
            // 게임 상태를 시작한 것으로 바꾼다.
            Hashtable props = new Hashtable
            {
                {_gameStartedPropKey, true }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        PV.RPC("LoadMultiPlayScene", RpcTarget.All);
    }

    [PunRPC]
    public void LoadMultiPlayScene()
    {
        PhotonNetwork.LoadLevel("MultiPlayScene");
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 방의 속성이 변경되었을 때 호출됩니다.
        if (propertiesThatChanged.ContainsKey(_gameStartedPropKey))
        {
            IsGameStarted();
        }
    }

    public void IsGameStarted()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(_gameStartedPropKey, out object isGameStarted))
        {
            if ((bool)isGameStarted)
            {
                // 게임이 시작된 상태입니다. 필요한 처리를 수행합니다.
                //Debug.Log("Game has started.");
                _isGameStarted = true;
                // 게임이 시작된 상태에서 필요한 로직을 여기에 추가합니다.
            }
            else
            {
                //Debug.Log("Game has not started.");
                _isGameStarted = false;
                // 게임이 시작되지 않은 상태에서 필요한 로직을 여기에 추가합니다.
            }
        }
        else
        {
            //Debug.Log("인터넷이 불안정");
            _isGameStarted = false;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("새로운 마스터 클라이언트: " + newMasterClient.ToString());

        PlayerStatus[] players = FindObjectsOfType<PlayerStatus>();

        foreach(PlayerStatus player in players) 
        {
            if(player._nickName == newMasterClient.NickName)
            {
                Debug.Log("새로운 마스터 클라이언트 -> 집주인 변신");

                // 새 마스터 클라이언트에게 Houseowner 역할 부여
                player.GetComponent<PhotonView>().RPC("SetRole", RpcTarget.AllBuffered, Define.Role.Houseowner);

                // 집주인 메시 및 애니메이션 전환 호출
                player.GetComponent<PhotonView>().RPC("TransformIntoHouseowner", RpcTarget.AllBuffered);
                player.GetComponent<PhotonView>().RPC("SetRoleAnimator", RpcTarget.AllBuffered);
            }
            else
            {
                if (player.GetComponent<PlayerStatus>().Hp <= 0) return; // 죽으면 이전 집주인이 강도가 되어서 죽지 않고 서있는 문제 방지

                // 나머지 플레이어들에게 Robber 역할 부여
                player.GetComponent<PhotonView>().RPC("SetRole", RpcTarget.AllBuffered, Define.Role.Robber);

                // 도둑 메시 및 애니메이션 전환 호출
                player.GetComponent<PhotonView>().RPC("TransformIntoRobber", RpcTarget.AllBuffered);
                player.GetComponent<PhotonView>().RPC("SetRoleAnimator", RpcTarget.AllBuffered);
                // Debug.Log("다름: " + player._nickName + " | " + newMasterClient.NickName);
            }
        }
    }
    #endregion

    public int GetPlayerCount()
    {
        return _cachePlayerList.Count;
    }
}
