using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI_S : MonoBehaviour
{
    GameObject _player;
    PlayerStatus_S _status;
    public WeaponManager_S _weaponManager;

    //UI Î≥??àò?ì§

    // ?ãúÍ∞?
    TextMeshProUGUI _timeSecond;
    float _timer;

    // ?ä§?Öå?ù¥?Ñ∞?ä§
    Slider _hpBar;
    Slider _spBar;

    // Î¨¥Í∏∞
    RawImage _weaponIcon;
    public Texture2D[] _weaponImages = new Texture2D[2];
    TextMeshProUGUI _currentBullet;
    TextMeshProUGUI _totalBullet;
    TextMeshProUGUI _currentMonster;

    // Ï°∞Ï???Ñ†
    GameObject _crossHair;
    GameObject _exitMenu;

    void Start()
    {
       _player = GameObject.Find("Player");
       _status = _player.GetComponent<PlayerStatus_S>();
       //_weaponManager = _player.GetComponent<WeaponManager>();

       // ?ãúÍ∞? ?ëú?ãú?ï† Í≥?
       _timeSecond = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
       // Hp, Sp ?ëú?ãú?ï† Í≥?
       _hpBar = transform.GetChild(1).GetComponent<Slider>();
       _spBar = transform.GetChild(2).GetComponent<Slider>();

       // Î¨¥Í∏∞ ?†ïÎ≥? ?ëú?ãú?ï† Í≥?
       _weaponIcon = transform.GetChild(3).GetChild(0).GetComponent<RawImage>();
       _currentBullet = transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
       _totalBullet = transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

       // Ï°∞Ï???Ñ† UI
       _crossHair = transform.GetChild(5).gameObject;

       // ?òÑ?û¨ ?ú†?†π?ùò ?àò
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
        // Ï≤¥Î†•?ù¥ 0?ù¥Î©? Î©àÏ∂îÍ∏?

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
        Debug.Log("?òÑ?û¨Î¨¥Í∏∞: " + weaponTag);
        if (weaponTag == "Gun") // ?õêÍ±∞Î¶¨ Î¨¥Í∏∞?ùº Í≤ΩÏö∞
        {
           if(!_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(true);
           if(!_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(true);
           if(!_crossHair.activeSelf) _crossHair.SetActive(true);

           DisplayWeaponIcon(1);
            DisplayGunInfo();
        }
        else // Í∑ºÏ†ë Î¨¥Í∏∞?ùº Í≤ΩÏö∞
        {
           DisplayWeaponIcon(0);
           if (_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(false);
           if (_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(false);
           if (_crossHair.activeSelf) _crossHair.SetActive(false);
        }
    }

    public void DisplayGunInfo()
    {
        _currentBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetCurrentBullet().ToString();    // ?òÑ?û¨ ?û•?†ï?êú ?ÉÑ?ïΩ
        _totalBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetTotalBullet().ToString();         // ?†ÑÏ≤? ?ÉÑ?ïΩ
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
