using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class WeaponManager : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus _playerStatus;

    [Tooltip("무기 전환 시 지연 시간을 설정")]
    public float _switchDelay = 1f;

    [Header("무기 관련")]
    [SerializeField] public GameObject _leftItemHand;           // 왼손에 있는 아이템 (자식: 탄창)
    [SerializeField] public GameObject _rightItemHand;          // 오른손에 있는 아이템 (자식: 무기)

    [Header("현재 무기 관련")]
    public int _selectedWeaponIdx = 0;
    public GameObject _selectedWeapon;
    public bool _isHoldGun;

    void Awake()
    {
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus>();
        InitRoleWeapon();
    }

    void Start()
    {
        //InitRoleWeapon();
    }

    void Update()
    {
        if (GetComponent<PhotonView>().IsMine && !_playerInputs.aim && !_playerInputs.reload) // 조준하지 않고, 장전하지 않을 때 무기 교체 가능
            WeaponSwitching(); // 무기 교체
    }

    public void InitRoleWeapon()
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트에서만 실행
        {
            if (_playerStatus.Role == Define.Role.Houseowner) // 집주인
            {
                _selectedWeaponIdx = 1;
                _playerStatus._weaponHolder = _playerStatus._weaponHolders[1];
                _selectedWeapon = transform.GetChild(1).gameObject;
                GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 무기 초기화 시점에도 모든 클라이언트에 동기화
            }
        }
        
        // 역할에 따른 첫 무기 설정
        if (_playerStatus.Role == Define.Role.Robber) // 강도
        {
            _selectedWeaponIdx = 0;
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[0];
            _selectedWeapon = transform.GetChild(0).gameObject;
            GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 무기 초기화 시점에도 모든 클라이언트에 동기화
        }
        Debug.Log("역할에 따른 무기 매니저 초기화 완료");
    }

    void WeaponSwitching()
    {
        int previousSelectedWeapon = _selectedWeaponIdx;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (_selectedWeaponIdx >= transform.childCount - 1)
                _selectedWeaponIdx = 0;
            else
                _selectedWeaponIdx++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_selectedWeaponIdx <= 0)
                _selectedWeaponIdx = transform.childCount - 1;
            else
                _selectedWeaponIdx--;
        }

        if (previousSelectedWeapon != _selectedWeaponIdx) // 마우스 휠로 무기 인덱스 바뀌면 교체
        {
            if (_playerStatus.Role == Define.Role.Robber)
                _selectedWeaponIdx = 0;

            GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 모든 플레이어에게 무기 변경 알림
        }
    }

    [PunRPC]
    void SelectWeapon(int weaponIndex)
    {
        Debug.LogWarning($"_selectedWeaponIdx({transform.root.GetChild(2).GetComponent<PlayerStatus>()._nickName}) :" + weaponIndex);
        _selectedWeaponIdx = weaponIndex;

        int idx = 0;
        foreach (Transform weapon in transform)
        {
            if (idx == weaponIndex)
            {
                weapon.gameObject.SetActive(true);
                _selectedWeapon = weapon.gameObject; // 현재 고른 무기 참조
                IsHoldGun();
                _playerStatus.ChangeIsHoldGun(_isHoldGun);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            idx++;
        }
    }

    public void UseSelectedWeapon()
    {
        // 굳이 Input을 앞에 해준 이유가 역할이 변할 때, _selectedWeapon이 null이 되는 경우를 Update 돌다가 감지하기 때문이다.

        if (_selectedWeapon == null) return;

        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && _selectedWeapon.tag == "Melee")
        {
            _selectedWeapon.GetComponent<Melee>().Use();
        }
        else if (_selectedWeapon.tag == "Gun")
        {
            _selectedWeapon.GetComponent<Gun>().Use();
        }
        else
        {
            Debug.Log("This weapon has none tag");
        }
    }

    void IsHoldGun()
    {
        if (_selectedWeapon.tag == "Gun")
            _isHoldGun = true;
        else if (_selectedWeapon.tag == "Melee")
            _isHoldGun = false;
        else
        {
            Debug.Log("This weapon has none tag");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 현재 선택된 무기 인덱스를 보내기
            stream.SendNext(_selectedWeaponIdx);
        }
        else
        {
            // 클라이언트에서 받은 무기 인덱스를 반영
            int receivedWeaponIndex = (int)stream.ReceiveNext();
            GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, receivedWeaponIndex); // 받은 인덱스를 RPC로 처리
        }
    }

    /// <summary>
    /// 무기 줍기
    /// </summary>
    void PickUp()
    {

    }

    /// <summary>
    /// 무기 버리기
    /// </summary>
    void Drop()
    {

    }
}