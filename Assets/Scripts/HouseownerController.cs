using Unity.Netcode;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;
#endif

/// <summary>
/// 집주인 고유의 기능만 존재
/// </summary>
public class HouseownerController : NetworkBehaviour
{
    PlayerStatus _playerStatus;
    [SerializeField] NewWeaponManager _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
    }

    void Start()
    {
        HouseownerInit();
        _weaponManager.InitRoleWeapon();
    }
    void Update()
    {
        if (!IsLocalPlayer) return;

        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;
        
        _weaponManager.UseSelectedWeapon();

    }

    void HouseownerInit()
    {
        _playerStatus.Role = Define.Role.Houseowner;

        Animator houseownerAnimator = gameObject.GetComponent<Animator>();
        RuntimeAnimatorController houseAnimController = houseownerAnimator.runtimeAnimatorController;
        Avatar houseAvatar = houseownerAnimator.avatar;
        
        // Player 객체에도 같은 애니메이터가 존재하므로 꼬이게 된다. 따라서 Houseowner의 애니메이터를 비워준다.
        houseownerAnimator.runtimeAnimatorController = null;
        houseownerAnimator.avatar = null;

        _playerStatus.SetRoleAnimator(houseAnimController, houseAvatar);
    }
}
