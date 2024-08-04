using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 강도 컨트롤러
/// </summary>
public class RobberController : MonoBehaviour
{
    PlayerStatus _playerStatus;
    [SerializeField] WeaponManager _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
    }
    void Start()
    {
        RobberInit(); // 강도 세팅
        //_weaponManager.InitRoleWeapon();
    }

    void Update()
    {

        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;

        _weaponManager.UseSelectedWeapon();
    }

    void RobberInit()
    {
        Animator robberAnimator = gameObject.GetComponent<Animator>();
        RuntimeAnimatorController robberAnimController = robberAnimator.runtimeAnimatorController;
        Avatar robberAvatar = robberAnimator.avatar;

        //// Player 객체에도 같은 애니메이터가 존재하므로 꼬이게 된다. 따라서 Robber의 애니메이터를 비워준다.
        //robberAnimator.runtimeAnimatorController = null;
        //robberAnimator.avatar = null;

        //_playerStatus.SetRoleAnimator(robberAnimController, robberAvatar);
    }
}