using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseownerConroller_S : MonoBehaviour
{
    PlayerStatus_S _playerStatus;
    [SerializeField] WeaponManager_S _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus_S>();
    }

    void Start()
    {
        HouseownerInit();
        _weaponManager.InitRoleWeapon();
    }
    void Update()
    {
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
        //houseownerAnimator.runtimeAnimatorController = null;
        //houseownerAnimator.avatar = null;

        _playerStatus.SetRoleAnimator(houseAnimController, houseAvatar);
    }
}
