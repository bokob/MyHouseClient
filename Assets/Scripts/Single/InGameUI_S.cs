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

    bool _isDead = false;

    #region UI 관련
    // 생존시간
    TextMeshProUGUI _timeSecond;
    float _timer;

    // Hp, Sp
    Slider _hpBar;
    Slider _spBar;

    // 무기
    RawImage _weaponIcon;
    public List<Texture2D> _weaponImages = new List<Texture2D>();
    TextMeshProUGUI _currentBullet;
    TextMeshProUGUI _totalBullet;
    TextMeshProUGUI _currentMonster;

    // 조준점
    GameObject _crossHair;

    // 종료 메뉴
    GameObject _exitMenu;

    [Header("EndUI")]
    int score = 0;
    [SerializeField] float _endTime = 6f;
    [SerializeField] float _fadeDuration = 4.0f;
    [SerializeField] GameObject _endUI;
    [SerializeField] Image _fadeImageInPanel;
    [SerializeField] TextMeshProUGUI _killGhostText;
    [SerializeField] TextMeshProUGUI _quitText;

    #endregion


    void Start()
    {
       _player = GameObject.Find("Player");
       _status = _player.GetComponent<PlayerStatus_S>();

        InitUI();
    }

    void InitUI()
    {
        // 생존 시간
        _timeSecond = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        // Hp, Sp 바
        _hpBar = transform.GetChild(1).GetComponent<Slider>();
        _spBar = transform.GetChild(2).GetComponent<Slider>();

        // 무기 정보 표시
        _weaponIcon = transform.GetChild(3).GetChild(0).GetComponent<RawImage>();
        _currentBullet = transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        _totalBullet = transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        
        // 조준점
        _crossHair = transform.GetChild(5).gameObject;

        // 몬스터 수
        _currentMonster = transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>();
        
        // 종료 메뉴
        _exitMenu = transform.GetChild(6).gameObject;
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
            DisplayMonsterCount();
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
            _quitText.text = Mathf.FloorToInt(_endTime) + " seconds to quit.";
        }
    }

    public void DisplayLivingTime()
    {
        _timer += Time.deltaTime;
        _timeSecond.text = ((int)_timer).ToString();
    }

    public void DisplayHp() => _hpBar.value = _status.Hp / 100;

    public void DisplaySp() => _spBar.value = _status.Sp / 100;

    public void DisplayWeaponInfo()
    {
        string weaponTag = _weaponManager._selectedWeapon.tag;
        Debug.Log("현재 무기: " + weaponTag);
        if (weaponTag == "Gun") // 원거리 무기인 경우
        {
           if(!_currentBullet.gameObject.activeSelf) _currentBullet.gameObject.SetActive(true);
           if(!_totalBullet.gameObject.activeSelf) _totalBullet.gameObject.SetActive(true);
           if(!_crossHair.activeSelf) _crossHair.SetActive(true);

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
        _currentBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetCurrentBullet().ToString();    // ?��?�� ?��?��?�� ?��?��
        _totalBullet.text = _weaponManager._selectedWeapon.GetComponent<Gun_S>().GetTotalBullet().ToString();         // ?���? ?��?��
    }

    public void DisplayWeaponIcon(int iconIndex) => _weaponIcon.texture = _weaponImages[iconIndex];

    public void DisplayMonsterCount() => _currentMonster.text = GameManager_S._instance._monsterCount.ToString();
    public void DisableUI()
    {
        if (_status.Hp <= 0)
        {
            for(int i=0; i<transform.childCount; i++)
            {
                if (i == transform.childCount - 1) break;
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
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
    }

    // 게임 종료 스크린 보여주기
    IEnumerator ShowDeadScreen()
    {
        _endUI.SetActive(true);
        score = GameManager_S._instance._score; // 점수 지정
        _killGhostText.text = "Killed Ghost : " + score.ToString();

        float elapsedTime = 1.0f;
        Color color = _fadeImageInPanel.color;
        color.a = 0.0f; // 투명

        // 알파값 0에서 1로 증가
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 1.0f, elapsedTime / _fadeDuration);
            _fadeImageInPanel.color = color;
            yield return null;
        }

        color.a = 1.0f; // 불투명
        _fadeImageInPanel.color = color;
    }

    public void ExitToTitle() =>  SceneManager.LoadScene("TitleScene");
}