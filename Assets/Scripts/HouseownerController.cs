using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;
#endif

// 진짜 집주인 컨트롤러
public class HouseownerController : MonoBehaviour
{
    PlayerController _playerController;
    WeaponManager _weaponManager;

    [Tooltip("카메라")]
    GameObject _cameras;
    GameObject _quaterFollowCamera;
    GameObject _thirdFollowCamera;
    GameObject _aimCamera;

    void Awake()
    {
        _playerController = transform.parent.GetComponent<PlayerController>();
        _weaponManager = transform.parent.GetComponent<WeaponManager>();
    }

    void Start()
    {
        HouseownerInit();
    }
    void Update()
    {
        // 시체면 가만히 있게 하기
        if (_playerController.PlayerRole == Define.Role.None) return;

        // 총 관련해서 행동 안하고 있을 때만 무기 바꾸기
        if (!_playerController._input.aim && !_playerController._input.reload)
            _weaponManager.HandleWeaponSwitching();

        if (_weaponManager._melee.activeSelf) // 근접 무기가 있는 경우에만 공격
            _playerController.MeleeAttack();
        else if (_weaponManager._gun.activeSelf)
            _weaponManager._gunWeapon.Use();

        // 총 관련해서 행동 안하고 있을 때만 무기 바꾸기
        if (!_playerController._input.aim && !_playerController._input.reload)
            _weaponManager.HandleWeaponSwitching();
    }

    void HouseownerInit()
    {
        _playerController.PlayerRole = Define.Role.Houseowner;

        Animator houseownerAnimator = gameObject.GetComponent<Animator>();
        RuntimeAnimatorController houseAnimController = houseownerAnimator.runtimeAnimatorController;
        Avatar houseAvatar = houseownerAnimator.avatar;
        
        // Player 객체에도 같은 애니메이터가 존재하므로 꼬이게 된다. 따라서 Houseowner의 애니메이터를 비워준다.
        houseownerAnimator.runtimeAnimatorController = null;
        houseownerAnimator.avatar = null;

        _playerController.SetRoleAnimator(houseAnimController, houseAvatar);

        CameraInit();
    }

    void CameraInit() // 카메라 세팅
    {
        // 카메라 오브젝트 세팅
        _cameras = Camera.main.gameObject.transform.parent.gameObject;
        _quaterFollowCamera = _cameras.transform.GetChild(1).gameObject;
        _thirdFollowCamera = _cameras.transform.GetChild(2).gameObject;
        _aimCamera = _cameras.transform.GetChild(3).gameObject;

        // 집주인에 맞는 카메라 설정
        _quaterFollowCamera.SetActive(false);
        _thirdFollowCamera.SetActive(true);
        _aimCamera.SetActive(true);
    }
}
