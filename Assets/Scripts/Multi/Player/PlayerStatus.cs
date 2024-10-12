using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class PlayerStatus : MonoBehaviourPunCallbacks, IStatus
{
    public PlayerMove _playerMove;
    public CameraController _cameraController;
    public InGameUI _inGameUI;

    #region 상태, 능력치 및 이름
    public bool _isLocalPlayer;
    public string _nickName;
    [field: SerializeField] public Define.Role Role { get; set; } = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float Sp { get; set; } = 100;    // 스테미나
    [field: SerializeField] public float MaxHp { get; set; } = 100; // 최대 체력
    [field: SerializeField] public float MaxSp { get; set; } = 100; // 최대 스테미나
    [field: SerializeField] public float Defence { get; set; } = 1; // 방어력
    #endregion

    #region 애니메이션 및 피해
    public Animator _animator;
    List<Renderer> _renderers;
    #endregion

    public Transform _weaponHolder;
    public Transform[] _weaponHolders;
    public WeaponManager _weaponManager;

    public GameObject _smokeEffect;

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
    public void SetNickname(string _name) // 닉네임 설정
    {
        _nickName = _name;
    }
    
    [PunRPC]
    public void SetRole(Define.Role role) // 역할 지정하고 그에 맞게 변신
    {
        // 역할을 직접 할당
        Debug.Log($"내 역할({_nickName}): " + Role);

        Role = role;

        if (Role == Define.Role.Robber)
            _animator = transform.GetChild(0).GetComponent<Animator>();
        else if (Role == Define.Role.Houseowner)
            _animator = transform.GetChild(1).GetComponent<Animator>();


        transform.GetChild(0).gameObject.SetActive(Role == Define.Role.Robber); // 강도 활성화
        transform.GetChild(1).gameObject.SetActive(Role == Define.Role.Houseowner);  // 집주인 비활성화

        Debug.Log("현재 상태: " + Role);

        if(Role == Define.Role.Robber)
        {
            _cameraController.gameObject.GetComponent<CameraController>().SetRobberView(); // 강도 시점으로 설정
            Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(이)가 강도로 변신 완료");
            MaxSp = 200;
            Sp = MaxSp;
        }
        else if(Role == Define.Role.Houseowner)
        {
            _inGameUI.gameObject.transform.GetChild(4).gameObject.SetActive(true); // 조준점 활성화
            transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().ResetAlphaObjectsToBeHouseowner(); // 투명화 오브젝트들 초기화
            transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().enabled = false; // 강도 층별 투명화 비활성화
            _cameraController.gameObject.GetComponent<CameraController>().SetHouseownerView(); // 집주인 시점으로 설정

            MaxSp = 100;
            Sp = MaxSp;

            Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(이)가 집주인으로 변신 완료");
        }
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
            StartCoroutine(TombCoroutine());
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
            }
        }
    }

    void Update()
    {
        //if (!IsLocalPlayer) return;
        //Dead();
    }

    /// <summary>
    /// 멀티용 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    [PunRPC]
    public void TakedDamage(int attack, PhotonMessageInfo info)
    {
        // 해당 오브젝트의 소유자가 아니면 실행하지 않음
        if (!photonView.IsMine) return;

        Debug.Log("RPC 보낸 클라이언트: " + info.Sender.NickName);

        // 피해 계산
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;

        Debug.LogWarning($"{_nickName}({Role})(이)가 {info.Sender.NickName}에게 {damage} 만큼 피해 입음");
        Debug.LogWarning("남은 체력: " + Hp);

        if (Hp <= 0)
        {
            // 오직 소유자만 사망 처리를 진행
            if (photonView.IsMine)
            {
                Player killer = info.Sender;
                Debug.LogWarning($"{killer.NickName}가 {photonView.Owner.NickName}를 죽임");
                PhotonNetwork.SetMasterClient(killer);
            }

            // 사망 상태를 모든 클라이언트에 전파 (모든 클라이언트에서 죽음을 동기화)
            GetComponent<PhotonView>().RPC("Dead", RpcTarget.AllBuffered);
        }
    }

    /// <summary>
    /// 멀티에서 몬스터에게 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    public void TakedDamage(int attack)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;
        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);


        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "가" + damage + " 만큼 피해입음");
            Debug.Log("데미지 입고 난 후 체력: " + Hp);
        }
        else
        {
            Dead();
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
        Sp -= Time.deltaTime * 10;
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

    IEnumerator TombCoroutine()
    {
        SmokeEffect(transform.position);
        GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        yield return new WaitForSeconds(2f);
        transform.GetChild(0).gameObject.SetActive(false); // 강도 비활성화
        transform.GetChild(1).gameObject.SetActive(false);  // 집주인 비활성화
        transform.GetChild(3).gameObject.SetActive(true); // Tombstone Active
        yield return new WaitForSeconds(3f);
        if (GetComponent<PhotonView>().IsMine)
        {
            Application.Quit(); // 빌드 후에 게임 꺼버리기
            Time.timeScale = 0f;    // 엔진에서 테스트할 때 멈추게하기
            Debug.Log("게임 강제 종료");
        }
        else Debug.Log("종료 안됨");
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
            //Debug.Log("색변한다.");
            //Debug.Log(_renderers[i].material.name);
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

    [PunRPC]
    void SmokeEffect(Vector3 position)
    {
        GameObject smoke = PhotonNetwork.Instantiate("SmokeParticlePrefab", position, Quaternion.identity);
    }
}