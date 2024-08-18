using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class InGameUI_S : MonoBehaviour
{
    GameObject _player;
    PlayerStatus_S _status;
    public WeaponManager_S _weaponManager;

    //UI �??��?��

    // ?���?
    TextMeshProUGUI _timeSecond;
    float _timer;

    // ?��?��?��?��?��
    Slider _hpBar;
    Slider _spBar;

    // 무기
    RawImage _weaponIcon;
    public List<Texture2D> _weaponImages = new List<Texture2D>();
    TextMeshProUGUI _currentBullet;
    TextMeshProUGUI _totalBullet;
    TextMeshProUGUI _currentMonster;

    // 조�???��
    GameObject _crossHair;
    GameObject _exitMenu;

    void Start()
    {
       _player = GameObject.Find("Player");
       _status = _player.GetComponent<PlayerStatus_S>();
       //_weaponManager = _player.GetComponent<WeaponManager>();

       // ?���? ?��?��?�� �?
       _timeSecond = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
       // Hp, Sp ?��?��?�� �?
       _hpBar = transform.GetChild(1).GetComponent<Slider>();
       _spBar = transform.GetChild(2).GetComponent<Slider>();

       // 무기 ?���? ?��?��?�� �?
       _weaponIcon = transform.GetChild(3).GetChild(0).GetComponent<RawImage>();
       _currentBullet = transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
       _totalBullet = transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

       // 조�???�� UI
       _crossHair = transform.GetChild(5).gameObject;

       // ?��?�� ?��?��?�� ?��
       _currentMonster = transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>();

       _exitMenu = transform.GetChild(6).gameObject;
    }

    void Update()
    {
        DisplayMonsterCount();
        if (_status.Hp > 0)
        {
            DisplayLivingTime();
            DisplayHp();
            DisplaySp();
            DisplayWeaponInfo();
            DisplayExitMenu();
        }
        else
        {
            DisplayOut();
        }
    }

    public void DisplayLivingTime()
    {
        // 체력?�� 0?���? 멈추�?

        _timer += Time.deltaTime;
        _timeSecond.text = ((int)_timer).ToString();
    }

    public void DisplayHp()
    {
        _hpBar.value = _status.Hp / 100;
    }

    public void DisplaySp()
    {
        _spBar.value = _status.Sp / 100;
    }

    public void DisplayWeaponInfo()
    {
        string weaponTag = _weaponManager._selectedWeapon.tag;
        Debug.Log("?��?��무기: " + weaponTag);
        if (weaponTag == "Gun") // ?��거리 무기?�� 경우
        {
           if(!_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(true);
           if(!_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(true);
           if(!_crossHair.activeSelf) _crossHair.SetActive(true);

           DisplayWeaponIcon(1);
           DisplayGunInfo();
        }
        else // 근접 무기?�� 경우
        {
           DisplayWeaponIcon(GetWeaponIconIndex(_weaponManager._selectedWeapon.name));
           if (_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(false);
           if (_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(false);
           if (_crossHair.activeSelf) _crossHair.SetActive(false);
        }
    }


    public void DisplayGunInfo()
    {
        _currentBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetCurrentBullet().ToString();    // ?��?�� ?��?��?�� ?��?��
        _totalBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetTotalBullet().ToString();         // ?���? ?��?��
    }

    public void DisplayWeaponIcon(int iconIndex)
    {
        _weaponIcon.texture = _weaponImages[iconIndex];
    }

    public void DisplayMonsterCount()
    {
        _currentMonster.text = GameManager_S._instance._monsterCount.ToString();
    }
    public void DisplayOut()
    {
        if(_status.Hp <= 0) gameObject.SetActive(false);
    }

    // 무기 이름으로 무기 아이콘 구하기
    public int GetWeaponIconIndex(string weaponName)
    {
        int index = _weaponImages.Select((element, index) => new { element, index })
                        .FirstOrDefault(p => p.element.name == weaponName)
                        ?.index ?? 0;
        return index;
    public void DisplayExitMenu()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && _exitMenu.activeSelf == false)
        {
            _exitMenu.SetActive(true);
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && _exitMenu.activeSelf == true)
        {
            _exitMenu.SetActive(false);
        }
        if(Input.GetKeyDown(KeyCode.Return) && _exitMenu.activeSelf == true)
        {
            SceneManager.LoadScene("TitleScene");
        }
    }
    public void ExitToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void HideExitMenu()
    {
        _exitMenu.SetActive(false);
    }
}
