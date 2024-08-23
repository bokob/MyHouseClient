using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] GameObject _player;
    IStatus _status;
    [SerializeField] WeaponManager _robberWeaponManager;
    [SerializeField] WeaponManager _houseownerWeaponManager;
    [SerializeField] WeaponManager _weaponManager;
    [SerializeField] WeaponManager_S _weaponManagerS;

    bool _isDead = false;

    #region UI 변수

    [Header("공통 UI")]
    // 시간
    [SerializeField] TextMeshProUGUI _timeSecond;
    float _timer;

    // 스테이터스
    [SerializeField] Slider _hpBar;
    [SerializeField] Slider _spBar;

    // 무기
    public List<Texture2D> _weaponImages = new List<Texture2D>();
    RawImage _weaponIcon;
    TextMeshProUGUI _currentBullet;
    TextMeshProUGUI _totalBullet;

    // 조준선
    [SerializeField] GameObject _crossHair;

    // 종료 메뉴
    GameObject _exitMenu;

    [Header("EndUI")]
    int score = 0;
    [SerializeField] float _endTime = 6f;
    [SerializeField] float _fadeDuration = 4.0f;
    [SerializeField] GameObject _gameOverScreen;
    [SerializeField] Image _fadeImageInGameOverScreen;
    [SerializeField] TextMeshProUGUI _quitTimer;
    [SerializeField] TextMeshProUGUI _killGhostText;

    [Header("모드별 UI")]
    [SerializeField] List<Texture2D> _rightUpImages = new List<Texture2D>();
    RawImage _rightUpIcon;
    [SerializeField] TextMeshProUGUI _rightUpText;  // 싱글(현재 유령 수) 멀티(현재 접속 인원)
    #endregion

    void Start()
    {
        _status = _player.GetComponent<IStatus>();
        InitUI();
    }

    void InitUI()
    {
        // 생존 시간
        _timeSecond = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        // Hp, Sp 바
        _hpBar = transform.GetChild(1).GetComponent<Slider>();
        _spBar = transform.GetChild(2).GetComponent<Slider>();

        // 무기 정보
        _weaponIcon = transform.GetChild(3).GetChild(0).GetComponent<RawImage>();
        _currentBullet = transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        _totalBullet = transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

        // 조준선 UI
        _crossHair = transform.GetChild(4).gameObject;

        // 우상단 표시
        _rightUpIcon = transform.GetChild(5).GetChild(0).GetComponent<RawImage>();
        _rightUpText = transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>();

        // 게임 종료 메뉴
        _exitMenu = transform.GetChild(6).gameObject;

        // 죽은 화면
        _gameOverScreen = transform.GetChild(7).gameObject;
        _fadeImageInGameOverScreen = _gameOverScreen.GetComponent<Image>();
        _quitTimer = _gameOverScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _killGhostText = _gameOverScreen.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        AliveUI();
        JustNowDeadUI();
        CountQuitGame();
    }


    void AliveUI()
    {
        if (_status.Hp > 0 && !_isDead)
        {
            DisplayRightUp();
            DisplayExitMenu();
            DisplayLivingTime();
            DisplayHp();
            DisplaySp();
            DisplayWeaponInfo();
        }
    }

    void JustNowDeadUI()
    {
        if (_status.Hp <= 0 && !_isDead)
        {
            _isDead = true;
            DisableUI();
            StartCoroutine(ShowDeadScreen());
        }
    }

    void CountQuitGame()
    {
        if (_status.Hp <= 0 && _isDead)
        {
            _endTime -= Time.deltaTime;
            _quitTimer.text = Mathf.FloorToInt(_endTime) + " seconds to quit.";
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
        string weaponTag;
        if (NetworkManager._instance == null) // 싱글
        {
            weaponTag = _weaponManagerS._selectedWeapon.tag;
        }
        else // 멀티
        {
            _weaponManager = (_status.Role == Define.Role.Robber) ? _robberWeaponManager : _houseownerWeaponManager;
            weaponTag = _weaponManager._selectedWeapon.tag;
        }

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

            if (NetworkManager._instance == null) // 싱글
            {
                DisplayWeaponIcon(GetWeaponIconIndex(_weaponManagerS._selectedWeapon.name));
            }
            else // 멀티
            {
                DisplayWeaponIcon(GetWeaponIconIndex(_weaponManager._selectedWeapon.name));
            }

            if (_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(false);
            if (_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(false);
            if (_crossHair.activeSelf) _crossHair.SetActive(false);
        }
    }

    public void DisplayGunInfo()
    {
        if(NetworkManager._instance == null) // 싱글
        {
            _currentBullet.text = _weaponManagerS._selectedWeapon.GetComponent<Gun_S>().GetCurrentBullet().ToString();    // 현재 장정된 탄약
            _totalBullet.text = _weaponManagerS._selectedWeapon.GetComponent<Gun_S>().GetTotalBullet().ToString();         // 전체 탄약
        }
        else // 멀티
        {
            _currentBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun>().GetCurrentBullet().ToString();    // 현재 장정된 탄약
            _totalBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun>().GetTotalBullet().ToString();         // 전체 탄약
        }   
    }

    public void DisplayWeaponIcon(int iconIndex) => _weaponIcon.texture = _weaponImages[iconIndex];

    // 우상단 패널
    public void DisplayRightUp() 
    {
        _rightUpText.text = (NetworkManager._instance == null) ? GameManager_S._instance._monsterCount.ToString() : PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        _rightUpIcon.texture = (NetworkManager._instance == null) ? _rightUpImages[0] : _rightUpImages[1];
    }
    // 무기 이름으로 무기 아이콘 구하기
    public int GetWeaponIconIndex(string weaponName)
    {
        int index = _weaponImages.Select((element, index) => new { element, index })
                        .FirstOrDefault(p => p.element.name == weaponName)
                        ?.index ?? 0;
        return index;
    }

    public void DisplayExitMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _exitMenu.activeSelf)
        {
            _exitMenu.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !_exitMenu.activeSelf)
        {
            _exitMenu.SetActive(true);
        }

        if (_exitMenu.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            ExitToTitle();
        }
    }

    public void DisableUI()
    {
        if (_status.Hp <= 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i == transform.childCount - 1) break;
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    // 게임 종료 스크린 보여주기
    IEnumerator ShowDeadScreen()
    {
        _gameOverScreen.SetActive(true);

        if (NetworkManager._instance == null)
        {
            score = GameManager_S._instance._score; // 점수 지정
            _killGhostText.text = "Killed Ghost : " + score.ToString();
        }
        float elapsedTime = 1.0f;
        Color color = _fadeImageInGameOverScreen.color;
        color.a = 0.0f; // 투명

        // 알파값 0에서 1로 증가
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 1.0f, elapsedTime / _fadeDuration);
            _fadeImageInGameOverScreen.color = color;
            yield return null;
        }

        color.a = 1.0f; // 불투명
        _fadeImageInGameOverScreen.color = color;
    }

    public void ExitToTitle() => SceneManager.LoadScene("TitleScene");
}