using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using SlimUI.ModernMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방 목록에 나오는 방을 표시하는 UI 클래스
/// </summary>
public class RoomListItemUI : MonoBehaviour
{
    public string _roomName;         // 방 이름
    public TMP_Text _roomTitle;      // 방 제목
    public TMP_Text _playerCount;    // 플레이어 수

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            FindObjectOfType<UIMenuManager>().LobbyToRoomCamPos();
        });
    }

    public void SetRoom(RoomInfo room) // 방 설정
    {
        if (room.Name == null)
            Debug.Log("이름이 없음");

        _roomName = room.Name;
        _roomTitle.text = _roomName;                                  // 방 목록에 표시되는 방 이름
        _playerCount.text = "" + room.PlayerCount.ToString() + "/4";  // 방 목록에서 표시되는 해당 방의 플레이어 수
    }

    public void OnJoinPressed()
    {
        NetworkManager._instance._roomNameToJoin = _roomName;   // 참가할 방 이름 설정
        NetworkManager._instance._roomName.text = _roomName;    // 방 이름 설정
        Debug.Log(_roomName + " 방 버튼을 눌러서 방에 참가합니다.");
        NetworkManager._instance.JoinRoom();
    }
}
