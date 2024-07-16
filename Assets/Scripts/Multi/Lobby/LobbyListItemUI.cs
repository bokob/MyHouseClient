using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 목록에 나오는 로비를 표시하는 UI 클래스
/// </summary>
public class LobbyListItemUI : MonoBehaviour
{
    public TMP_Text lobbyText;      // 로비 이름
    public TMP_Text playerCount;    // 플레이어 수
    Lobby lobby;

    private void Awake()
    {
        // 클릭하면 로비에 들어갈 수 있게 버튼에 이벤트 추가
        GetComponent<Button>().onClick.AddListener(() => {
            FindObjectOfType<LobbyBrowseUI>().JoinLobbyById(lobby.Id);
        });
    }
    public void SetLobby(Lobby _lobby)
    {
        lobby = _lobby;                                                 // 매개변수로 받은 로비 정보를 필드 변수인 lobby에 저장
        lobbyText.text = lobby.Name;                                    // 로비 목록에 표시되는 로비 이름
        playerCount.text = "" + lobby.Players.Count.ToString() + "/4";  // 로비 목록에 표시되는 해당 로비 플레이어 수
    }
}
