using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Tooltip("무기 전환 시 지연 시간을 설정")]
    public float _switchDelay = 1f;

    [Tooltip("플레이어가 사용할 수 있는 무기 목록")]
    public List<GameObject> _weaponList = new List<GameObject>();

    int _index = 0;
    bool _isSwitching = false;

    PlayerController _playerController;
    RobberController _robberController;
    HouseownerController _houseownerController;

    [Header("무기 관련")]
    [SerializeField] public GameObject _leftItemHand;           // 왼손에 있는 아이템 (자식: 탄창)
    [SerializeField] public GameObject _rightItemHand;          // 오른손에 있는 아이템 (자식: 무기)
    [SerializeField] public GameObject _melee;                  // 근접 무기 오브젝트
    [SerializeField] public GameObject _gun;
    public Melee _meleeWeapon; // 근접 무기
    public Gun _gunWeapon;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _robberController = transform.GetChild(0).GetComponent<RobberController>();
        _houseownerController = transform.GetChild(1).GetComponent<HouseownerController>();
    }

    public void PlayerWeaponInit() // 무기 세팅
    {
        // 무기 리스트 비우기
        ClearWeaponList();

        // 무기 찾기
        // 하위의 모든 자식 오브젝트 중 "Hand" 태그를 갖는 활성화된 오브젝트만 찾기
        GameObject[] itemHand = GameObject.FindGameObjectsWithTag("Hand");

        // 이름순으로 정렬
        Array.Sort(itemHand, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        if (itemHand.Length < 2)
        {
            Debug.LogError("Both left and right hand objects are not found.");
            return;
        }

        _leftItemHand = itemHand[0];
        _rightItemHand = itemHand[1];

        // 근접 무기 접근을 위한 변수 설정
        _melee = _rightItemHand.transform.GetChild(0).gameObject;

        _meleeWeapon = _melee.GetComponent<Melee>();
        AddWeaponInList(_melee);

        // 원거리 무기 접근을 위한 변수 설정
        if (_rightItemHand.transform.childCount == 2) // 오른손에 무언가 있는 경우
        {
            _gun = _rightItemHand.transform.GetChild(1).gameObject;
            _gunWeapon = _gun.GetComponent<Gun>();
            AddWeaponInList(_gun);
        }
    }

    // 무기 전환 입력을 처리하는 메서드
    public void HandleWeaponSwitching()
    {
        if (!_isSwitching)
        {
            // 마우스 휠 위로 스크롤 시 다음 무기로 전환
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                _index = (_index + 1) % _weaponList.Count;
                StartCoroutine(SwitchDelay(_index));
            }
            // 마우스 휠 아래로 스크롤 시 이전 무기로 전환
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                _index = (_index - 1 + _weaponList.Count) % _weaponList.Count;
                StartCoroutine(SwitchDelay(_index));
            }

            // 숫자 키(1~9)로 무기 전환
            for (int i = 49; i < 58; i++)
            {
                int keyIndex = i - 49;
                if (Input.GetKeyDown((KeyCode)i) && _weaponList.Count > keyIndex && _index != keyIndex)
                {
                    _index = keyIndex;
                    StartCoroutine(SwitchDelay(_index));
                }
            }
        }
    }

    // 무기 목록에 새로운 무기를 추가
    public void AddWeaponInList(GameObject weaponObject)
    {
        if (weaponObject != null && !_weaponList.Contains(weaponObject))
            _weaponList.Add(weaponObject);
    }

    // 무기 목록에서 특정 무기를 제거
    public void RemoveWeaponInList(GameObject weaponObject)
    {
        if (weaponObject != null && _weaponList.Contains(weaponObject))
            _weaponList.Remove(weaponObject);
    }

    public void ClearWeaponList()
    {
        _weaponList.Clear();
    }

    // 무기를 초기 상태로 설정
    public void InitializeWeapon()
    {
        ClearWeaponList();
        PlayerWeaponInit();

        // 모든 무기를 비활성화
        foreach (GameObject weapon in _weaponList)
        {
            if (weapon != null)
                weapon.SetActive(false);
        }

        // 역할에 따른 첫 무기 설정
        if(_playerController.PlayerRole == Define.Role.Houseowner) // 집주인
        {
            //houseownerController.ChangeIsHoldGun(true);
            _index = Mathf.Clamp(1, 0, _weaponList.Count - 1); // 인덱스가 리스트 범위 내에 있는지 확인
            if (_weaponList.Count > _index)
                _weaponList[_index].SetActive(true);
        }
        else if(_playerController.PlayerRole == Define.Role.Robber) // 강도
        {
            _index = Mathf.Clamp(0, 0, _weaponList.Count - 1);
            if (_weaponList.Count > _index)
                _weaponList[_index].SetActive(true);
        }

        Debug.Log("무사히 무기 세팅 완료");
    }

    // 무기 전환 시 지연 시간을 추가하여 무한 전환 방지
    IEnumerator SwitchDelay(int newIndex)
    {
        _isSwitching = true;
        SwitchWeapons(newIndex);
        yield return new WaitForSeconds(_switchDelay);
        _isSwitching = false;
    }

    void SwitchWeapons(int newIndex)
    {
        // 모든 무기를 비활성화
        for (int i = 0; i < _weaponList.Count; i++)
        {
            if (_weaponList[i] != null)
                _weaponList[i].SetActive(false);
        }

        // 무기 인덱스가 유효한지 확인
        if (newIndex < 0 || newIndex >= _weaponList.Count || _weaponList[newIndex] == null)
            return;

        // 근접 무기인지 여부에 따라 집주인의 무기 상태를 변경
        if (_weaponList[newIndex].CompareTag("Melee"))
        {
            if (_houseownerController != null)
                _playerController.ChangeIsHoldGun(false);
        }
        else
        {
            if (_houseownerController != null)
                _playerController.ChangeIsHoldGun(true);
        }

        // 새로운 무기 활성화
        _weaponList[newIndex].SetActive(true);
    }

    // 현재 들고 있는 무기의 태그 구하기
    public string GetCurrentWeaponTag()
    {
        return _weaponList[_index].tag;
    }
}