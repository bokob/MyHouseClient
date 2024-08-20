using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] PlayerStatus _status;
    [SerializeField] WeaponManager _robberWeaponManager;
    [SerializeField] WeaponManager _houseownerWeaponManager;
    WeaponManager _weaponManager;

    #region UI 변수
    // 접속 인원
    [SerializeField] TextMeshProUGUI _connectedPeople;

    // 시간
    [SerializeField] TextMeshProUGUI _timeSecond;
    float _timer;

    // 스테이터스
    [SerializeField] Slider _hpBar;
    [SerializeField] Slider _spBar;

    // 무기
    RawImage _weaponIcon;
    public Texture2D[] _weaponImages = new Texture2D[2];
    [SerializeField] TextMeshProUGUI _currentBullet;
    [SerializeField] TextMeshProUGUI _totalBullet;

    // 조준선
    [SerializeField] GameObject _crossHair;
    #endregion

    void Start()
    {
        // 접속 인원
        _connectedPeople = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        // 시간 표시할 곳
        _timeSecond = transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        // Hp, Sp 표시할 곳
        _hpBar = transform.GetChild(2).GetComponent<Slider>();
        _spBar = transform.GetChild(3).GetComponent<Slider>();

        // 무기 정보 표시할 곳
        _weaponIcon = transform.GetChild(4).GetChild(0).GetComponent<RawImage>();
        _currentBullet = transform.GetChild(4).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        _totalBullet = transform.GetChild(4).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

        // 조준선 UI
        _crossHair = transform.GetChild(5).gameObject;
    }

    void Update()
    {
        if(_status.Hp > 0)
        {
            DisplayConnectedPlayers();
            DisplayLivingTime();
            DisplayHp();
            DisplaySp();
            DisplayWeaponInfo();
        }
        else
        {
            DisplayOut();
        }
    }

    public void DisplayLivingTime()
    {
        // 체력이 0이면 멈추기
        if (_status.Hp <= 0) return;

        _timer += Time.deltaTime;
        _timeSecond.text = ((int)_timer).ToString();
    }

    public void DisplayHp() => _hpBar.value = _status.Hp / 100;

    public void DisplaySp() => _spBar.value = _status.Sp / 100;

    public void DisplayWeaponInfo()
    {
        _weaponManager = (_status.Role == Define.Role.Robber) ? _robberWeaponManager : _houseownerWeaponManager;
        string weaponTag = _weaponManager._selectedWeapon.tag;
        Debug.Log("현재 무기: " + weaponTag);
        if (weaponTag == "Gun") // 원거리 무기인 경우
        {
            if (!_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(true);
            if (!_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(true);
            if (!_crossHair.activeSelf) _crossHair.SetActive(true);

            DisplayWeaponIcon(1);
            DisplayGunInfo();
        }
        else // 근접 무기인 경우
        {
            DisplayWeaponIcon(GetWeaponIconIndex(_weaponManager._selectedWeapon.name));
            if (_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(false);
            if (_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(false);
            if (_crossHair.activeSelf) _crossHair.SetActive(false);
        }
    }

    public void DisplayGunInfo()
    {
        _currentBullet.text =  _weaponManager._selectedWeapon.GetComponent<Gun>().GetCurrentBullet().ToString();    // 현재 장정된 탄약
        _totalBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun>().GetTotalBullet().ToString();         // 전체 탄약
    }

    public void DisplayWeaponIcon(int iconIndex) => _weaponIcon.texture = _weaponImages[iconIndex];

    public void DisplayConnectedPlayers() => _connectedPeople.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();

    // 무기 이름으로 무기 아이콘 구하기
    public int GetWeaponIconIndex(string weaponName)
    {
        int index = _weaponImages.Select((element, index) => new { element, index })
                        .FirstOrDefault(p => p.element.name == weaponName)
                        ?.index ?? 0;
        return index;
    }

    public void DisplayOut()
    {
        if (_status.Hp <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
    }
}

