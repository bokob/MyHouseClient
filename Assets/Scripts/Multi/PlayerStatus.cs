using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class PlayerStatus : MonoBehaviourPunCallbacks
{
    public PlayerMove _playerMove;
    public CameraController _cameraController;
    public InGameUI _inGameUI;

    #region 상태 및 능력치, 이름 등
    public bool _isLocalPlayer;
    public string _nickName;
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float Sp { get; set; } = 100;    // 스테미나
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // 최대 체력
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // 최대 스테미나
    [field: SerializeField] public float Defence { get; private set; } = 1; // 방어력
    #endregion

    #region 애니메이션 및 피해
    public Animator _animator;
    List<Renderer> _renderers;
    #endregion

    public Transform _weaponHolder;
    public Transform[] _weaponHolders;

    //public string nickname;

    //public TextMeshPro nicknameText;

    //public Transform TPWeaponHolder;

    public void IsLocalPlayer()
    {
        //TPWeaponHolder.gameObject.SetActive(false);

        _isLocalPlayer = true;
        _playerMove.enabled = true;         // PlayerMove 활성화
        _cameraController.gameObject.transform.parent.gameObject.SetActive(true);
        _inGameUI.gameObject.SetActive(true);
        transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().enabled = true; // 강도 층별 투명화 활성화
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        _nickName = _name;
    }

    [PunRPC]
    public void SetRole(Define.Role role) // 역할 지정
    {
        // 역할을 직접 할당
        Debug.Log($"내 역할({_nickName}): " + Role);

        Role = role;

        if (Role == Define.Role.Robber)
            _animator = transform.GetChild(0).GetComponent<Animator>();
        else if (Role == Define.Role.Houseowner)
            _animator = transform.GetChild(1).GetComponent<Animator>();
    }

    /// <summary>
    /// 사망
    /// </summary>
    [PunRPC]
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            Role = Define.Role.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    void Awake()
    {
        // 렌더 가져오기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("왜 색이 널?");
            }
        }
    }

    void Update()
    {
        //if (!IsLocalPlayer) return;
        //Dead();
    }

    /// <summary>
    /// 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    [PunRPC]
    public void TakedDamage(int attack, PhotonMessageInfo info)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;
        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);

        if(Hp <= 0)
        {
            Player killer = info.Sender;

            GameManager._instance.OnPlayerKilled(photonView.Owner, killer);

            GetComponent<PhotonView>().RPC("Dead", RpcTarget.AllBuffered);
        }

    }


    #region 체력 및 스테미나 관련
    /// <summary>
    /// 최대 체력의 0.2만큼 회복
    /// </summary>
    public void Heal()
    {
        // 현재 체력이 최대 체력보다 작을 때만 회복 적용
        if (Hp < MaxHp)
        {
            // 회복량
            float healAmount = MaxHp * 0.2f;

            // 회복량과 현재 체력과의 합이 최대 체력을 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("이전 체력" + Hp);
            // 체력 회복
            Hp += healedAmount;
            Debug.Log("체력을 " + healedAmount + "만큼 회복!");
            Debug.Log("현재 체력: " + Hp);
        }
        else
        {
            Debug.Log("최대 체력. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 최대 스테미나까지 전부 회복
    /// </summary>
    public void SpUp()
    {
        // 현재 스테미나가 최대 스테미나보다 작을 때만 회복 적용
        if (Sp < MaxSp)
        {
            // 회복량과 현재 스테미나와의 합이 최대 스테미나를 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("이전 스테미나" + Sp);
            // 스테미나 회복
            Sp += healedAmount;
            Debug.Log("전부 회복! 현재 Sp: " + Sp);
        }
        else
        {
            Debug.Log("최대 Sp. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 스테미나 차오르기
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 스테미나 깎이기
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 점프시, 스테미나 감소
    /// </summary>
    public void JumpSpDown() => Sp -= 3;

    /// <summary>
    /// 방어력 증가
    /// </summary>
    public void DefenceUp()
    {

    }
    #endregion


    /// <summary>
    /// 집주인으로 변신
    /// </summary>
    [PunRPC]
    public void TransformIntoRobber()
    {
        transform.GetChild(0).gameObject.SetActive(true); // 강도 활성화
        transform.GetChild(1).gameObject.SetActive(false);  // 집주인 비활성화

        Debug.Log("현재 상태: " + Role);

        _cameraController.gameObject.GetComponent<CameraController>().SetRobberView(); // 강도 시점으로 설정

        Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(이)가 강도로 변신 완료");
    }


    /// <summary>
    /// 집주인으로 변신
    /// </summary>
    [PunRPC]
    public void TransformIntoHouseowner()
    {
        transform.GetChild(0).gameObject.SetActive(false); // 강도 비활성화
        transform.GetChild(1).gameObject.SetActive(true);  // 집주인 활성화

        _inGameUI.gameObject.transform.GetChild(4).gameObject.SetActive(true); // 조준점 활성화

        transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().enabled = false; // 강도 층별 투명화 비활성화

        Debug.Log($"현재 상태({transform.root.GetChild(2).GetComponent<PlayerStatus>()._nickName}): " + Role);

        _cameraController.gameObject.GetComponent<CameraController>().SetHouseownerView(); // 집주인 시점으로 설정

        Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(이)가 집주인으로 변신 완료");
    }

    /// <summary>
    /// 시체 바닥으로 가라앉기
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -5f)
        {
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
        // Destroy(gameObject);

        if(GetComponent<PhotonView>().IsMine)
        {
            Application.Quit();
        }

        //Application.Quit(); // 게임 종료
        Debug.Log("게임 강제 종료");
    }

    /// <summary>
    /// 피해 받으면 Material 붉게 변화
    /// </summary>
    [PunRPC]
    public void HitChangeMaterials()
    {
        // 태그가 무기 또는 몬스터

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색변한다.");
            Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"플레이어가 {other.transform.root.name}에게 공격 받음!");
        Debug.Log("공격받은 측의 체력:" + Hp);
    }

    /// <summary>
    /// 피해 받고 Material 원래대로 복구
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }
}