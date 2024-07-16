using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 강도 컨트롤러
/// </summary>
public class RobberController : NetworkBehaviour
{
    PlayerStatus _playerStatus;
    [SerializeField] NewWeaponManager _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
    }
    void Start()
    {
        RobberInit(); // 강도 세팅
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;

        if (Input.GetKeyUp(KeyCode.T)) // 'T' 누르면 집주인으로 변신
            _playerStatus.TransformIntoHouseowner();

        _weaponManager.UseSelectedWeapon();
    }

    void RobberInit()
    {
        _playerStatus.Role = Define.Role.Robber;

        Animator robberAnimator = gameObject.GetComponent<Animator>();
        RuntimeAnimatorController robberAnimController = robberAnimator.runtimeAnimatorController;
        Avatar robberAvatar = robberAnimator.avatar;

        // Player 객체에도 같은 애니메이터가 존재하므로 꼬이게 된다. 따라서 Robber의 애니메이터를 비워준다.
        robberAnimator.runtimeAnimatorController = null;
        robberAnimator.avatar = null;

        _playerStatus.SetRoleAnimator(robberAnimController, robberAvatar);
    }
}