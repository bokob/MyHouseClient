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
    public static NetworkManager _instance = null;

    public const string _gameStartedPropKey = "IsGameStarted";
    public bool _isGameStarted;
    public string _nickName;
    public string _roomNameToJoin = "test"; // ������ �� �̸�

    [Header("DisconnectPanel")]
    public TMP_InputField _nickNameInput; // �г��� �̸�

    [Header("LobbyPanel")]
    public GameObject _lobbyPanel;
    public TextMeshPro _lobbyInfoText;    // �κ� ���� ������ UI(�� ���� �κ� �ִ���, �� ���� MyHouse�� �����ߴ���)
    public GameObject _roomListUI;        // �� ���(��� ǥ�õǴ� ���� ����Ŵ)
    public GameObject _roomListItemUI;    // �� (�� ��Ͽ� ǥ�õǴ� ���� ���� ����Ŵ)
    List<RoomInfo> _cacheRoomList = new List<RoomInfo>(); // �� ���� ��Ƶ� ����Ʈ


    [Header("RoomPanel")]
    public GameObject _roomPanel;
    public TextMeshPro _roomName;       // �� ����
    public TextMeshPro _roomInfoText;   // �� ���� ������ UI
    public GameObject[] _roomPlayers;     // �濡 ������ �÷��̾� ��Ÿ���� UI
    List<Player> _cachePlayerList = new List<Player>(); // �� ���� ��Ƶ� ����Ʈ
    public GameObject _startBtn;

    [Header("ä��")]
    public GameObject _chatListUI;
    public GameObject _chatListItemUI;
    public TMP_InputField _chatInput; // ä�� �Է�â

    [Header("ETC")]
    public TextMeshPro StatusText;
    public PhotonView PV;

    [Header("�˸� �޽��� ����")]
    public GameObject connectionResponseUI;             // '������' ��Ÿ���� UI
    public TMP_Text messsageText;                       // ���� ���� ����
    public GameObject connectionResponseCloseButton;    // ���� ���� ���� �� �ݴ� ��ư

    //[Header("���� ���� ����")]
    //public List<GameObject> _playerPrefabListInRoom = new List<GameObject>();


    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetNetworkManagerGameObject()
    {
        return gameObject;
    }

    #region ��������
    void Update()
    {
        if (!_isGameStarted)
        {
            _lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "Lobby / " + PhotonNetwork.CountOfPlayers + "Connected"; // �� ���� �����ֱ�

            // ���� ĥ ��, �濡 �ְ� ȭ��Ʈ �����̽��� �ƴϿ��� ��
            if (PhotonNetwork.InRoom && !string.IsNullOrWhiteSpace(_chatInput.text) && Input.GetKeyDown(KeyCode.Return))
            {
                Send();
            }
        }
    }

    // ���� ������ ����
    public void Connect()
    {
        // Debug.Log("���� ���� ����");

        UIMenuManager._instance.responseMain.SetActive(true);


        if(PhotonNetwork.ConnectUsingSettings())
        {
            //Debug.Log("���� ���� ����");
        }
        else
        {
            //Debug.Log("������ ���� ����");

            // ���� ȭ�鿡�� ���� ���� ���� Ȱ��ȭ
            UIMenuManager._instance.responseMain.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Server Connection Failed";
        }
    }

    // ������ �����ϰ�, �ݹ����� ȣ��� -> �κ� �����ϰ� ��
    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.JoinLobby())
        {
            // ���� ȭ�鿡�� ������ ���� ��Ȱ��ȭ

            Debug.Log("�κ� ���� ����");
            UIMenuManager._instance.responseMain.SetActive(false);
            UIMenuManager._instance.MainToLobbyCamPos();
        }
        else
        {
            Debug.Log("�κ� ���� ����");
            UIMenuManager._instance.responseMain.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Access Lobby";
        }
    }

    // �κ� �������� �� �ݹ�
    public override void OnJoinedLobby()
    {
        UIMenuManager._instance.responseRoom.SetActive(false);

        if (string.IsNullOrWhiteSpace(_nickNameInput.text))  // �ƹ��͵� �ԷµǾ� ���� ���� ���
        {
            _nickName = UnityEngine.Random.Range(100, 1000).ToString(); // ���ڸ� �������� �ο�
            _nickNameInput.text = _nickName;
        }
        _nickName = _nickNameInput.text;
        PhotonNetwork.LocalPlayer.NickName = _nickName;
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " ���� �κ� �����߽��ϴ�.");
        _cacheRoomList.Clear();
    }

    // ���� ���� ���� ����
    public void Disconnect()
    {
        Debug.Log("���� ���� ���� ����");

        if(SceneManager.GetActiveScene().name == "TitleScene")
            UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Discconecting to Server...";
        PhotonNetwork.Disconnect();
    }

    // ���� ���� ������, �ݹ����� ȣ���
    public override void OnDisconnected(DisconnectCause cause) 
    {
        //Debug.Log("���� ���� ����: " + cause.ToString());
        if(SceneManager.GetActiveScene().name == "TitleScene") 
        { 
            UIMenuManager._instance.responseLobby.SetActive(false);
            UIMenuManager._instance.LobbyToMainCamPos();
        }
    }
    #endregion

    #region ��

    // �� �����
    public void CreateRoom()
    {
        UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Creating a room...";
        UIMenuManager._instance.responseLobby.SetActive(true);

        // �� ����
        if (PhotonNetwork.CreateRoom(_roomName.text = "Room " + UnityEngine.Random.Range(100, 1000).ToString(), new RoomOptions { 
            // �� �ɼ�
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 4,     // �ִ� �ο� ��
            EmptyRoomTtl = 0    // ���� ��� ���� �� ��� ����  
        }))
        {
            Debug.Log("�� ���� ����");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            Debug.Log("�� ���� ����");
            UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Create Room";
        }
    }

    // ������ �������� ����
    public void JoinRandomRoom()
    {
        UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Quick Start...";
        if (PhotonNetwork.JoinRandomRoom())
        {
            //Debug.Log("���� ���� ����");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            //Debug.Log("���� ���� ����");
            UIMenuManager._instance.responseLobby.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Quick Start";
        }
    }

    // �� ������
    public void LeaveRoom()
    {

        //Debug.Log(PhotonNetwork.CurrentRoom.Name + " ��(��) �����մϴ�.");

        UIMenuManager._instance.responseRoom.SetActive(true);
        UIMenuManager._instance.responseRoom.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Leave Room...";
        
        if (PhotonNetwork.LeaveRoom())
        {
            //Debug.Log("�� ������ ����");
            _startBtn.SetActive(false);
            UIMenuManager._instance.RoomToLobbyCamPos();
        }
        else
        {
            UIMenuManager._instance.responseRoom.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Failed To Leave Room";
            //Debug.Log("�� ������ ����");
        }
    }

    // 1. ���� ����� �� �� 2. �ٷ� ������ �� �� �ݹ�
    public override void OnJoinedRoom()
    {
        IsGameStarted(); // ���� ���������� Ȯ��
        if (_isGameStarted)
        {
            PhotonNetwork.LoadLevel("MultiPlayScene");
            return; // ���� ���̸� �� ���� ���� �� ��
        }
        UIMenuManager._instance.responseLobby.SetActive(false);
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " �� �����մϴ�.");
        //RoomPanel.SetActive(true);
        _cachePlayerList.Clear();

        if (PhotonNetwork.IsMasterClient)
            _startBtn.SetActive(true);

        UpdateUserInRoomUI();
    }

    // �� ����� �������� �� �ݹ�
    public override void OnCreateRoomFailed(short returnCode, string message) 
    { 
        Debug.Log(returnCode.ToString() + message);

        _roomName.text = "Room"; 
        CreateRoom(); 
    }

    // �� ���� ���� �������� �� �ݹ�
    public override void OnJoinRandomFailed(short returnCode, string message) 
    {
        Debug.Log(returnCode.ToString() + message);

        _roomName.text = ""; 
        CreateRoom(); 
    }

    // �濡 �������� ��, �ȿ� �ִ� ��ο��� ����
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (_isGameStarted)
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObject in playerObjects)
            {
                PhotonView photonView = playerObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.Owner.IsMasterClient) // ������ Ŭ���̾�Ʈ
                {
                    // ������ �Ǿ� �ִٸ� (���� RPC ������ ���� ������Ʈ �ȵǴ� ���)
                    if (playerObject.GetComponent<PlayerStatus>().Role != Define.Role.Houseowner)
                    {
                        // �� ������ Ŭ���̾�Ʈ���� Houseowner ���� �ο�
                        photonView.RPC("SetRole", RpcTarget.AllBuffered, Define.Role.Houseowner);

                        // ������ �޽� �� �ִϸ��̼� ��ȯ ȣ��
                        //photonView.RPC("TransformIntoHouseowner", RpcTarget.AllBuffered);
                        photonView.RPC("SetRoleAnimator", RpcTarget.AllBuffered);
                    }

                    break;
                }
            }

            return; // ���� ���̸� �� ���� ���� �� ��
        }

        ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
        UpdateUserInRoomUI();
    }

    // ���� ���� ��, �ȿ� �ִ� ��ο��� ����
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (_isGameStarted)
        {
            UpdateCachePlayerList(); // �� �ȿ� �ִ� �÷��̾� ����Ʈ ������Ʈ, ������ ������ Ŭ���̾�Ʈ�� �����Ǹ� ���������� �ٲ�� �ϱ� ����

            //Debug.Log(otherPlayer.NickName + "(��)�� �������ϴ�.");

            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObject in playerObjects)
            {
                PhotonView photonView = playerObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.Owner == otherPlayer && photonView.IsMine) // �����̰�, �ش� �÷��̾��� �ڱ� �ڽ� �����ϰ� ����
                {
                    photonView.RPC("SmokeEffect", RpcTarget.All, playerObject.transform.position);
                    PhotonNetwork.Destroy(playerObject.transform.parent.gameObject);
                    //Destroy(playerObject.transform.parent.gameObject); // ������Ʈ ����
                    break;
                }
            }

            return; // ���� ���̸� �� ���� ���� �� ��
        }

        ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
        UpdateUserInRoomUI();
    }

    void UpdateUserInRoomUI() // ����� �÷��̾ �°� UI ������Ʈ
    {

        if (PhotonNetwork.IsMasterClient) // ���� ��ư ������ Ŭ���̾�Ʈ�� Ȱ��ȭ
        {
            _startBtn.SetActive(true);
        }

        UpdateCachePlayerList();

        // �÷��̾� ���� UI�� ������Ʈ
        for (int i=0; i< 4; i++)
        {
            if (i < _cachePlayerList.Count)
            {
                _roomPlayers[i].SetActive(true);
                _roomPlayers[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[i].NickName;
                Debug.Log(_cachePlayerList[i].NickName + "�� UI Ȱ��ȭ");
            }
            else
            {
                _roomPlayers[i].SetActive(false);
            }
        }
    }

    // �濡 ������ �÷��̾� ����Ʈ ����
    void UpdateCachePlayerList()
    {
        Player[] playerList = PhotonNetwork.PlayerList; // ���� �濡 �ִ� ��� �÷��̾� ��� ��������
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
                // �ʿ��� ��� ���� �÷��̾� ���� ������Ʈ
                int index = _cachePlayerList.IndexOf(player);
                _cachePlayerList[index] = player;
                Debug.Log("Player updated: " + player.NickName);
            }
        }

        // ����Ʈ�� �ִ� �÷��̾� �� �濡 ���� �÷��̾� ����
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
    /// �� ��ư ������ ����
    /// </summary>
    public void JoinRoom() 
    {
        //Debug.Log("�濡 ������...");

        if(PhotonNetwork.JoinOrCreateRoom(_roomNameToJoin, new RoomOptions
        { // �� �ɼ�
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 4,     // �ִ� �ο� ��
            EmptyRoomTtl = 0    // ���� ��� ���� �� ��� ����  
        }, null))
        {
            Debug.Log("�� ���� ����");
            UIMenuManager._instance.LobbyToRoomCamPos();
        }
        else
        {
            //Debug.Log("�� ���� ����");
        }

        /*
         �������̶�� ���� Ȱ��ȭ��ų ���� 
        */

        //nameUI.SetActive(false);
        //connectingUI.SetActive(true);
    }
    #endregion

    #region �渮��Ʈ ����
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // �� ����� �ٱ͸� ȣ��Ǵ� �ݹ�
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

    void UpdateRoomListUI() // �� ��� UI ������Ʈ
    {
        // ���� �� ��� UI �����ϱ�
        foreach (Transform child in _roomListUI.transform)
        {
            Destroy(child.gameObject);  // ���� �κ� ��� UI ��� ����
        }

        // �� UI ����
        foreach (RoomInfo room in _cacheRoomList)
        {
            GameObject _room = Instantiate(_roomListItemUI, _roomListUI.transform);   // �� ���ø� �����ؼ� ���� �� UI ����
            _room.SetActive(true);
            _room.GetComponent<RoomListItemUI>().SetRoom(room); // �� �κ� UI�� ���� �κ� ������ ����
        }
    }
    #endregion

    #region ä��
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + _chatInput.text); // �濡 �ִ� ��ο��� ���
        _chatInput.text = "";
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    void ChatRPC(string msg)
    {
        GameObject chatManager = Instantiate(_chatListItemUI, _chatListUI.transform);
        chatManager.GetComponent<ChatMessage>().SetText(msg);
        _chatListUI.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f; // ��ũ�� �� ���ϴ����� ���� (�ڵ� ��ũ��)
    }
    #endregion


    #region ���� ����

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("�ε�� ��: " + scene.name);

        //if (scene.name == "SinglePlayScene")
        //    Destroy(gameObject);


        // �÷��̾� ��ȯ
        if(scene.name == "MultiPlayScene")
            GameManager._instance.SapwnPlayer();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient) // ������ Ŭ���̾�Ʈ�� ���
        {
            // ���� ���¸� ������ ������ �ٲ۴�.
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
        // ���� �Ӽ��� ����Ǿ��� �� ȣ��˴ϴ�.
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
                // ������ ���۵� �����Դϴ�. �ʿ��� ó���� �����մϴ�.
                //Debug.Log("Game has started.");
                _isGameStarted = true;
                // ������ ���۵� ���¿��� �ʿ��� ������ ���⿡ �߰��մϴ�.
            }
            else
            {
                //Debug.Log("Game has not started.");
                _isGameStarted = false;
                // ������ ���۵��� ���� ���¿��� �ʿ��� ������ ���⿡ �߰��մϴ�.
            }
        }
        else
        {
            //Debug.Log("���ͳ��� �Ҿ���");
            _isGameStarted = false;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("���ο� ������ Ŭ���̾�Ʈ: " + newMasterClient.ToString());

        PlayerStatus[] players = FindObjectsOfType<PlayerStatus>();

        foreach(PlayerStatus player in players) 
        {
            if(player._nickName == newMasterClient.NickName)
            {
                Debug.Log("���ο� ������ Ŭ���̾�Ʈ -> ������ ����");

                // �� ������ Ŭ���̾�Ʈ���� Houseowner ���� �ο�
                player.GetComponent<PhotonView>().RPC("SetRole", RpcTarget.AllBuffered, Define.Role.Houseowner);

                // ������ �ִϸ��̼� ��ȯ ȣ��
                player.GetComponent<PhotonView>().RPC("SetRoleAnimator", RpcTarget.AllBuffered);
            }
            else
            {
                if (player.GetComponent<PlayerStatus>().Hp <= 0) return; // ������ ���� �������� ������ �Ǿ ���� �ʰ� ���ִ� ���� ����
            }
        }
    }
    #endregion
}
