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

    [Header("현재 무기 관련")]
    public int _selectedWeaponIdx = 0;
    public GameObject _selectedWeapon;
    public GameObject _recentMelee; // the most recent Melee
    public bool _isHoldGun;

    // 무기 아이템 관련
    public GameObject nearMeleeObject;
    public string meleeItemName;
    public bool _isPickUp = false; // 사거리 내에서 그 무기 주웠는지 안주웠는지 판단
    public bool _isUsePickUpWeapon = false; // 주운 무기를 사용하고 있는지 판단
    public int _pickUpWeaponIdx = 0;

    void Awake()
    {
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus>();
        InitRoleWeapon();

        _recentMelee = transform.GetChild(0).gameObject; // _recentMelee init
    }

    void Update()
    {
        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;

        UseSelectedWeapon();

        if (GetComponent<PhotonView>().IsMine && !_playerInputs.aim && !_playerInputs.reload) // 조준하지 않고, 장전하지 않을 때 무기 교체 가능
            WeaponSwitching(); // 무기 교체


        if (Input.GetKeyDown(KeyCode.E) && nearMeleeObject != null && !_playerInputs.reload)
        {
            _isPickUp = true;
            meleeItemName = nearMeleeObject.name;
            PickUpWeapon(meleeItemName);
        }

        if (GetComponent<PhotonView>().IsMine && Input.GetKeyDown(KeyCode.Q))
            DropWeapon();
    }

    public void InitRoleWeapon()
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트에서만 실행
        {
            if (_playerStatus.Role == Define.Role.Houseowner) // 집주인
            {
                _selectedWeaponIdx = 1;
                _playerStatus._weaponHolder = _playerStatus._weaponHolders[1];
                _playerStatus._weaponManager = _playerStatus._weaponHolder.GetComponent<WeaponManager>();
                _selectedWeapon = transform.GetChild(1).gameObject;
                GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 무기 초기화 시점에도 모든 클라이언트에 동기화
            }
        }
        
        // 역할에 따른 첫 무기 설정
        if (_playerStatus.Role == Define.Role.Robber) // 강도
        {
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[0];
            _playerStatus._weaponManager = _playerStatus._weaponHolder.GetComponent<WeaponManager>();
            _selectedWeapon = transform.GetChild(0).gameObject;
            /* 
             강도는 무기 초기화 하면 안 됨, 왜냐하면 뒤늦게 접속한 클라이언트 때문에 전부 초기화 됨
             그렇다고 무기 스위칭을 해당 객체만 하게끔하면, 무기 교체가 안됨
            */
        }
        Debug.Log("역할에 따른 무기 매니저 초기화 완료");
    }

    void WeaponSwitching()
    {
        if (_playerStatus.Role == Define.Role.Robber && _isUsePickUpWeapon) return;

        int previousSelectedWeapon = _selectedWeaponIdx;

        if (_selectedWeapon.tag == "Melee") // if now pick weapon tag is Melee
        {
            _recentMelee = _selectedWeapon;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_selectedWeapon == _recentMelee)
                _selectedWeaponIdx = 1;
            else if (_selectedWeapon.tag == "Gun")
                _selectedWeaponIdx = _recentMelee.transform.GetSiblingIndex();
        }

        if (previousSelectedWeapon != _selectedWeaponIdx) // 마우스 휠로 무기 인덱스 바뀌면 교체
        {
            if (_playerStatus.Role == Define.Role.Robber && !_isUsePickUpWeapon)
                _selectedWeaponIdx = 0;

            GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 모든 플레이어에게 무기 변경 알림
        }
    }

    // 무기 선택(해당 무기 활성화)
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

                if (_playerStatus == null) Debug.LogError($"playerStatus가 널, {GetComponent<PhotonView>().ViewID}");
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
        if (_selectedWeapon == null) return;

        // 굳이 Input을 앞에 해준 이유가 역할이 변할 때, _selectedWeapon이 null이 되는 경우를 Update 돌다가 감지하기 때문이다.
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && _selectedWeapon.tag == "Melee")
        {
            _selectedWeapon.GetComponent<Melee>().Use();
        }
        else if (_selectedWeapon.tag == "Gun")
        {
            _selectedWeapon.GetComponent<Gun>().Use();
        }
    }

    void IsHoldGun()
    {
        if (_selectedWeapon.tag == "Gun")
            _isHoldGun = true;
        else if (_selectedWeapon.tag == "Melee")
            _isHoldGun = false;
        else
            Debug.Log("This weapon has none tag");
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
    public void PickUpWeapon(string meleeName)
    {
        Transform newMelee = transform.Find(meleeName);
        _selectedWeaponIdx = newMelee.GetSiblingIndex(); // 교체할 무기가 몇 번째 자식인지
        _pickUpWeaponIdx =_selectedWeaponIdx;
        GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx);
    }

    /// <summary>
    /// 무기 버리기
    /// </summary>
    void DropWeapon()
    {
        if (_selectedWeapon.tag == "Gun" || _selectedWeaponIdx == 0) return;

        GameObject droppedSelectedWeapon = PhotonNetwork.Instantiate(_selectedWeapon.name, _selectedWeapon.transform.position, _selectedWeapon.transform.rotation); // instatntiation. 
        droppedSelectedWeapon.transform.localScale = droppedSelectedWeapon.transform.localScale * 1.7f; // size up.

        StartCoroutine(DropAndBounce(droppedSelectedWeapon));

        _selectedWeaponIdx = 0;
        GetComponent<PhotonView>().RPC("SelectWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx); // 모든 플레이어에게 무기 변경 알림
    }

    IEnumerator DropAndBounce(GameObject droppedSelectedWeapon)
    {
        float floorY = transform.root.GetChild(2).position.y + 0.3f; // floorY is Player object's position.y + 0.3f.

        Vector3 velocity = new Vector3(0, -1f, 0); // first velocity.
        float gravity = -9.8f;
        float bounceDamping = 0.6f;
        float horizontalDamping = 0.98f;

        while (true)
        {
            droppedSelectedWeapon.transform.position += velocity * Time.deltaTime;


            if (droppedSelectedWeapon.transform.position.y <= floorY)
            {
                //bouncing.
                droppedSelectedWeapon.transform.position = new Vector3(droppedSelectedWeapon.transform.position.x, floorY, droppedSelectedWeapon.transform.position.z);
                velocity.y = -velocity.y * bounceDamping;

                velocity.x *= horizontalDamping;
                velocity.z *= horizontalDamping;

                if (Mathf.Abs(velocity.y) < 0.1f)
                {
                    velocity.y = 0;
                    break;
                }
            }
            else
            {
                // gravity.
                velocity.y += gravity * Time.deltaTime;
            }

            yield return null; // wait for next frame.
        }

        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(droppedSelectedWeapon);
    }
}