using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager instance;
    public const int MAX_PLAYERS = 4;
    public GameObject playerPrefab;     // 게임 시작 시 스폰될 플레이어

    public NetworkList<PlayerData> playerDataNetworkList;           // 네트워크 상에서 동기화되는 플레이어 데이터 목록
    public delegate void OnPlayerDataListChanged();                 // 플레이어 데이터 목록이 변경될 때 호출되는 델리게이트
    public static OnPlayerDataListChanged onPlayerDataListChanged;

    string username;            // 플레이어 이름
    public enum GameState // only allow players to join while waiting to start
    {
        WaitingToStart,
        InHub,
        InGame,
        End
    }
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private void Awake()
    {
        // 싱글톤 만들기
        if (NetworkGameManager.instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        playerDataNetworkList = new NetworkList<PlayerData>();
        DontDestroyOnLoad(gameObject);

        username = PlayerPrefs.GetString("USERNAME", "Guest: " + Random.Range(100, 1000));
    }

    private void Start()
    {
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged; // 플레이어 데이터 목록의 변경 이벤트 핸들러 설정
    }

    public string GetUsername() // 플레이어 이름 가져오기
    {
        return username;
    }
    public void SetUsername(string _username) // 플레이어 이름 설정하기
    {
        if (string.IsNullOrWhiteSpace(_username))
        {
            username = "Guest: " + Random.Range(100, 1000);
        }
        else
        {
             username = _username;
        }

        PlayerPrefs.SetString("USERNAME", username);
    }

    public string GetUsernameFromClientId(ulong _clientId) // 클라이언트 ID를 통해 사용자명 가져오기
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == _clientId)
                return playerData.username.ToString();
        }
        return default;
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent) // 플레이어 데이터 목록이 변경될 때 호출
    {
        //Debug.Log("Invoke");
        //Debug.Log(playerDataNetworkList.Count);
        onPlayerDataListChanged?.Invoke();
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }
    public PlayerData GetPlayerDataFromIndex(int _playerIndex)
    {
        return playerDataNetworkList[_playerIndex];
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }
        return default;
    }
    public PlayerData GetLocalPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }
    public int GetPlayerDataIndexFromClientID(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }
        return -1;
    }

    public void StartHost() // 호스트를 시작하고, 연결 승인 콜백, 클라이언트 연결 및 해제 콜백을 설정, 로비 씬 로드
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += Network_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_Server_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();
        LoadLobbyJoinedScene();
    }

    private void Network_Server_OnClientDisconnectCallback(ulong _clientId) // 서버에서 클라이언트가 연결 해제될 때 호출
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData data = playerDataNetworkList[i];
            if (data.clientId == _clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }

        if (SceneManager.GetActiveScene().name == "MultiPlayScene")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }
    private void Network_Server_OnClientConnectedCallback(ulong _clientId) // 서버에서 클라이언트가 연결될 때 호출
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = _clientId,
        });
        SetUsernameServerRpc(GetUsername());
    }
    public void StartClient() // 클라이언트 시작, 클라이언트 연결 및 해제 콜백 설정
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Client_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void Network_Client_OnClientConnectedCallback(ulong obj) // 클라이언트가 연결되었을 때 호출
    {
        SetUsernameServerRpc(GetUsername());
        if (SceneManager.GetActiveScene().name == "MultiPlayScene")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }
    [ServerRpc(RequireOwnership = false)] // 클라이언트가 소유하지 않은 네트워크 오브젝트에서도 이 서버 RPC를 호출할 수 있음을 나타낸다.
    void SetUsernameServerRpc(string _username, ServerRpcParams rpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientID(rpcParams.Receive.SenderClientId);
        PlayerData data = playerDataNetworkList[playerIndex];
        data.username = _username;
        playerDataNetworkList[playerIndex] = data;
    }

    private void Network_OnClientDisconnectCallback(ulong clientId) // 클라이언트 연결이 해제되었을 때 호출
    {
        //Debug.Log("2");
        if (SceneManager.GetSceneByName("LoadingScene") == SceneManager.GetActiveScene())
        {
            // failed to connect
            FindObjectOfType<LobbyBrowseUI>().ConnectionFailed();
        }
        else if (SceneManager.GetSceneByName("RoomScene") == SceneManager.GetActiveScene())
        {
            // inside a lobby;
            FindObjectOfType<LobbyJoinedUI>().LeaveLobbyPressed(); // 로비 떠나기
        }
        else
        {
            // ingame
            //UI.instance.EnableHostDisconnectTab();
        }

        //throw new System.NotImplementedException();
    }

    /// <summary>
    /// 새로운 클라이언트가 서버에 연결을 시도할 때 해당 연결 요청을 승인할지 여부를 결정하는 콜백 함수
    /// </summary>
    /// <param name="connectionApprovalRequest"></param>
    /// <param name="connectionApprovalResponse"></param>
    void Network_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //Debug.Log("1");
        if (gameState.Value != GameState.WaitingToStart)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started.";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYERS)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full.";
            return;
        }
        connectionApprovalResponse.Approved = true;
        //connectionApprovalResponse.CreatePlayerObject = true; 
    }

    void LoadLobbyJoinedScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("RoomScene", LoadSceneMode.Single);
    }

    public void LoadGameScene() // 게임 씬 호출
    {
        LobbyManager.instance.DeleteLobby(); // 게임 중인 방은 로비 목록에 안뜨게 하니까 그냥 지워버린다. (나중에 삭제 안시키고, 다른 유저가 접속할 수 있게 해야 함)

        //string map = PlayerPrefs.GetString("ZOMBIES_MAP", "LAB");
        NetworkManager.Singleton.SceneManager.LoadScene("MultiPlayScene", LoadSceneMode.Single);
    }

    public void SpawnPlayers() // server
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) // 연결된 클라이언트 ID 순회
        {
            GameObject player = Instantiate(playerPrefab); // 플레이어 오브젝트 생성

            // clientId는 플레이어 오브젝트를 '소유'할 클라이언트 ID
            // true는 플레이어 오브젝트를 전역적으로 스폰하겠다는 뜻, 이를 통해 모든 클라이언트가 이 오브젝트를 인식하게 된다.
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
}