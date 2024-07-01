using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;                // 싱글톤 인스턴스
    const string KEY_RELAY_JOIN_CODE = "RelayJoinCode"; // 로비 데이터에 저장되는 Relay Join 코드를 위한 키 상수

    Lobby joinedLobby;                                  // 플레이어가 현재 참가한 로비

    float heartBeatTimer;                               // 로비의 히트비트 타이머 
    float heartBeatTimerMax = 15;                       // 로비의 히트비트 타이머 최대값

    float listLobbiesTimer;                             // 로비 목록 갱신 타이머
    float listLobbiesTimerMax = 3;                      // 로비 목록 갱신 타이머 최대값

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged; // 로비 목록이 변경될 때 발생하는 이벤트
    public class OnLobbyListChangedEventArgs : EventArgs    // 이벤트 인자로 로비 목록을 전달하기 위한 클래스
    {
        public List<Lobby> lobbyList;
    }
    private void Awake()
    {
        // 싱글톤으로 만들기
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 히트비트 타이머, 로비 목록 갱신 타이머 최대값으로 초기화
        heartBeatTimer = heartBeatTimerMax;
        listLobbiesTimer = listLobbiesTimerMax;

        InitializeUnityAuthentication();
    }

    /// <summary>
    /// 익명 인증 초기화
    /// </summary>
    async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) // Unity 서비스가 초기화 되지 않았을 경우
        {
            // InitializationOptions 객체 생성
            InitializationOptions options = new InitializationOptions();

            // 프로필 설정, 100에서 999 사이의 무작위 숫자를 문자열로 변환하여 프로필 이름으로 사용
            options.SetProfile(UnityEngine.Random.Range(100, 1000).ToString());

            // Unity 서비스 초기화, 비동기적으로 처리되므로 await 키워드를 사용하여 완료를 기다린다.
            await UnityServices.InitializeAsync(options);

            // 익명 인증, 비동기적으로 처리되므로 await 키워드를 사용하여 완료를 기다린다.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /// <summary>
    /// Relay 할당 요청
    /// </summary>
    /// <returns></returns>
    async Task<Allocation> AllocateRelay()
    {
        try
        {
            // Relay 서비스 인스턴스를 사용하여 할당 생성
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(NetworkGameManager.MAX_PLAYERS - 1);
            return allocation; // 생성된 할당 반환
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);  // 예외 메세지 출력
            return default; // 기본 값 반환
        }
    }

    /// <summary>
    /// 할당된 Relay join 코드 요청
    /// </summary>
    /// <param name="allocation"></param>
    /// <returns></returns>
    async Task<string> GetRelayJoinCode(Allocation allocation) // Task<string> 반환 형식은 이 메서드가 비동기적으로 문자열(join 코드)을 반환할 것을 의미
    {
        try
        {
            // Relay 서비스 인스턴스를 사용하여 할당 ID를 기반으로 join 코드를 가져온다.
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayCode; // join 코드 반환
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);  // 예외 메세지 출력
            return default; // 기본 값 반환
        }
    }

    /// <summary>
    /// Relay join 코드를 사용하여 Relay에 참여
    /// </summary>
    /// <param name="joinCode"></param>
    /// <returns></returns>
    async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            // Relay 서비스 인스턴스를 사용하여 join 코드를 통해 할당에 연결
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation; // 연결된 할다 정보 반환
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex); // 예외 메시지 출력
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 로비 연결 오류 메시지 UI에 전달
            return default; // 기본 값 반환

        }
    }

    /// <summary>
    /// 로비 생성, Relay 할당하여 로비 데이터 업데이트,
    /// 네트워크 매니저 설정, 호스트 시작
    /// </summary>
    /// <param name="lobbyName"></param>
    /// <param name="isPrivate"></param>
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            // 로비 생성
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, NetworkGameManager.MAX_PLAYERS, new CreateLobbyOptions
            {
                IsPrivate = isPrivate // 공개 여부
            });

            // Relay 할당 생성
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation); // Relay 서버에 연결할 때 사용할 join 코드 

            // 로비 데이터를 업데이트하여 join 코드를 포함
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            // NetworkManager의 UnityTransport 구성 요소에 Relay 서버 데이터 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            // 게임 서버(방장)를 호스트로 시작
            NetworkGameManager.instance.StartHost();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex); // 예외 메시지 출력
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 로비 연결 오류 메시지 UI에 전달
        }
    }

    /// <summary>
    /// 자동으로 로비에 빠르게 참여
    /// </summary>
    public async void QuickJoin()
    {
        try
        {
            // 현재 활성화된 로비 중 하나에 빠르게 참여시킴
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value; // 참여할 로비 데이터에서 Relay 서버에 연결할 때 사용할 join 코드
            JoinAllocation joinAllocation = await JoinRelay(relayCode); // join 코드를 사용하여 Relay 할당에 연결

            // NetworkManager의 UnityTransport 구성 요소에 Relay 서버 데이터 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            // 클라이언트로서 게임 서버에 연결
            NetworkGameManager.instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex); // 오류 메시지 출력
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 로비 연결 오류 메시지 UI에 전달
        }
    }

    /// <summary>
    /// 특정 로비 코드를 사용하여 로비에 참여
    /// </summary>
    /// <param name="lobbyCode"></param>
    public async void JoinByCode(string lobbyCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(lobbyCode)) // 로비 코드가 비어 있거나 공백일 경우
            {
                Debug.LogError("Lobby code cannot be empty or contain white space.");
                LobbyBrowseUI.instance.LobbyConnectError("Lobby code cannot be empty");
                return;
            }

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode); // 로비 코드로 로비 참여

            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value; // 참여한 로비 데이터에서 Relay 서버에 연결할 때 사용할 join 코드 가져오기
            JoinAllocation joinAllocation = await JoinRelay(relayCode); // join 코드를 사용하여 Relay 할당에 연결

            // NetworkManager의 UnityTransport 구성 요소에 Relay 서버 데이터 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            // 클라이언트로서 게임 서버에 연결
            NetworkGameManager.instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex); // 오류 메시지 출력
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 로비 연결 오류 UI에 전달
        }
    }

    /// <summary>
    /// 특정 로비 ID를 사용하여 로비에 참여 (로비 리스트에서 클릭해서 접속할 때 사용)
    /// </summary>
    /// <param name="lobbyID"></param>
    public async void JoinByID(string lobbyID)
    {
        try
        {
            // 로비 ID로 로비에 참여
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);

            // 참여한 로비 데이터에서 Relay 서버에 연결할 때 사용할 join 코드를 가져온다.
            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayCode); // join 코드를 사용하여 Relay 할당에 연결

            // NetworkManager의 UnityTransport 구성 요소에 Relay 서버 데이터를 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            // 클라이언트로서 게임 서버에 연결
            NetworkGameManager.instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex); // 오류 메시지 출력
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 로비 연결 오류 UI에 전달
        }
    }

    /// <summary>
    /// 현재 로비 떠나기
    /// </summary>
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                // 플레이어 목록에서 제거, 자기 자신 ID 뿐만 아니라 다른 플레이어의 ID도 지정 가능(강퇴 기능 만들 수 있음)
                // 플레이어가 남아 있으면 남아 있는 플레이어 중 한 명이 호스트 플레이어가 됨
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex); // 오류 메시지 출력
            }
        }
    }

    /// <summary>
    /// 현재 로비 삭제
    /// </summary>
    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id); // 로비 삭제
                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
    }

    /// <summary>
    /// 로비 목록을 쿼리하고 이벤트를 통해 로비 목록이 변경되었음을 알림
    /// </summary>
    async void ListLobbies()
    {
        try
        {
            // 쿼리 옵션 설정
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    // 이용 가능한 슬롯(플레이어가 참여할 수 있는 여유 슬롯)이 0보다 큰 로비들만 필터링
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },

                // 생성 시간 기준으로 내림차순 정렬
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created)
                }
            };

            // 로비 서비스를 사용하여 로비 쿼리를 비동기적으로 실행
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);

            // 로비 목록이 변경되었음을 이벤트를 통해 알림
            // 이벤트가 null이 아닌 경우에만 호출된다. 또, 이벤트 핸들러가 등록되어 있지 않으면 호출되지 않는다.
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results // 쿼리 결과인 로비 목록
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private void Update()
    {
        LobbyHeartBeat();  // 하트비트 타이머 갱신
        LobbyListUpdate(); // 로비 목록 갱신
    }

    /// <summary>
    /// 로비 호스트일 경우 주기적으로 하트비트 핑을 보내 로비 상태 유지
    /// </summary> 
    void LobbyHeartBeat()
    {
        if (IsLobbyHost()) // 현재 로비에 대해 사용자가 호스트인지
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0)
            {
                heartBeatTimer = heartBeatTimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id); // 여전히 로비가 활성 상태임을 알린다. (이거 안하고 밑 줄에 삭제되게 응용 가능)
            }
        }
    }

    /// <summary>
    /// 로비 검색 창에 계속 있으면 로비 검색 창 업데이트
    /// </summary>
    void LobbyListUpdate()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn &&
            SceneManager.GetSceneByName("LoadingScene") == SceneManager.GetActiveScene())
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0)
            {
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    /// <summary>
    /// 현재 플레이어가 로비 호스트인지 확인
    /// </summary>
    /// <returns></returns>
    bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    /// <summary>
    /// 현재 참여한 로비 반환
    /// </summary>
    /// <returns></returns>
    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }
}
