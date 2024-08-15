using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour
{
    #region ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½É·ï¿½Ä¡
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // Ã¼ï¿½ï¿½
    [field: SerializeField] public float Sp { get; set; } = 100;    // ï¿½ï¿½ï¿½×¹Ì³ï¿½
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // ï¿½Ö´ï¿½ Ã¼ï¿½ï¿½
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // ï¿½Ö´ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½
    [field: SerializeField] public float Defence { get; private set; } = 1; // ï¿½ï¿½ï¿½ï¿½
    #endregion

    #region ï¿½Ö´Ï¸ï¿½ï¿½Ì¼ï¿½ ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    [Header("EndUI")]
    public int score = 0;
    public float endTime = 6f;
    public float fadeDuration = 4.0f;
    public GameObject endUI;
    public Image fadeImage;
    public TextMeshProUGUI endText;
    public TextMeshProUGUI quitText;
    bool _dead;

    WeaponManager_S _weaponManager_S;
    private GameObject nearMeleeObject;
    private string meleeItemName;

    void Awake()
    {
        _animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        InitRole();
        endUI.SetActive(false);

        _weaponManager_S = transform.root.GetComponentInChildren<WeaponManager_S>();

        // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½?");
            }
        }
    }

    void Update()
    {
        Dead();
        if(_dead)
        {
            endTime -= Time.deltaTime;
            quitText.text = Mathf.FloorToInt(endTime) + " seconds to quit.";
        }

        if (Input.GetKeyDown(KeyCode.P) && nearMeleeObject != null && _weaponManager_S._selectedWeapon.tag != "Gun")
        {
            GetMeleeItem();
        }
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ ï¿½Ê±ï¿½È­
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        È£ï¿½ï¿½Æ®ï¿½ï¿½, Houseownerï¿½ï¿½ï¿½ï¿½ ï¿½Ï°ï¿½, Å¬ï¿½ï¿½ï¿½Ì¾ï¿½Æ®ï¿½ï¿½ Robber

        ï¿½Ì±ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Î¸ï¿½
         */
        Role = Define.Role.Houseowner;
    }



    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ô±ï¿½
    /// </summary>
    /// <param name="attack"> ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½İ·ï¿½ </param>
    public void TakedDamage(int attack)
    {
        if (Role == Define.Role.None) return; // ?‹œì²´ì¼ ê²½ìš° ì¢…ë£Œ

        // ?”¼?•´ê°? ?Œ?ˆ˜?¼ë©? ?šŒë³µë˜?Š” ?˜„?ƒ?´ ?¼?–´?‚˜ë¯?ë¡? ?”¼?•´?˜ ê°’ì„ 0?´?ƒ?œ¼ë¡? ?˜ê²Œë” ?„¤? •
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "(?´)ê°? " + damage + " ë§Œí¼ ?”¼?•´ë¥? ?…?—ˆ?Œ!");
            Debug.Log("?‚¨??? ì²´ë ¥: " + Hp);
        }
        else
        {
            Dead();
        }
    }

    /// <summary>
    /// ï¿½Ö´ï¿½ Ã¼ï¿½ï¿½ï¿½ï¿½ 0.2ï¿½ï¿½Å­ È¸ï¿½ï¿½
    /// </summary>
    public void Heal()
    {
        // ï¿½ï¿½ï¿½ï¿½ Ã¼ï¿½ï¿½ï¿½ï¿½ ï¿½Ö´ï¿½ Ã¼ï¿½Âºï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ È¸ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        if (Hp < MaxHp)
        {
            // È¸ï¿½ï¿½ï¿½ï¿½
            float healAmount = MaxHp * 0.2f;

            // È¸ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ Ã¼ï¿½Â°ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ö´ï¿½ Ã¼ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Êµï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("ï¿½ï¿½ï¿½ï¿½ Ã¼ï¿½ï¿½" + Hp);
            // Ã¼ï¿½ï¿½ È¸ï¿½ï¿½
            Hp += healedAmount;
            Debug.Log("Ã¼ï¿½ï¿½ï¿½ï¿½ " + healedAmount + "ï¿½ï¿½Å­ È¸ï¿½ï¿½!");
            Debug.Log("ï¿½ï¿½ï¿½ï¿½ Ã¼ï¿½ï¿½: " + Hp);
        }
        else
        {
            Debug.Log("ï¿½Ö´ï¿½ Ã¼ï¿½ï¿½. È¸ï¿½ï¿½ï¿½ï¿½ ï¿½Ê¿ï¿½ ï¿½ï¿½ï¿½ï¿½.");
        }
    }

    /// <summary>
    /// ï¿½Ö´ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ È¸ï¿½ï¿½
    /// </summary>
    public void SpUp()
    {
        // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½ï¿½ï¿½ ï¿½Ö´ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ È¸ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        if (Sp < MaxSp)
        {
            // È¸ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ö´ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Êµï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½×¹Ì³ï¿½" + Sp);
            // ï¿½ï¿½ï¿½×¹Ì³ï¿½ È¸ï¿½ï¿½
            Sp += healedAmount;
            Debug.Log("ï¿½ï¿½ï¿½ï¿½ È¸ï¿½ï¿½! ï¿½ï¿½ï¿½ï¿½ Sp: " + Sp);
        }
        else
        {
            Debug.Log("ï¿½Ö´ï¿½ Sp. È¸ï¿½ï¿½ï¿½ï¿½ ï¿½Ê¿ï¿½ ï¿½ï¿½ï¿½ï¿½.");
        }
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½×¹Ì³ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½×¹Ì³ï¿½ ï¿½ï¿½ï¿½Ì±ï¿½
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½, ï¿½ï¿½ï¿½×¹Ì³ï¿½ ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// ï¿½ï¿½ï¿?
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _dead = true;
            Role = Define.Role.None; // ?‹œì²?
            _animator.SetTrigger("setDie");
            StartCoroutine(DeadSinkCoroutine());

            // ê²Œì„ ? •?‚°
            endUI.SetActive(true);
            score = GameManager_S._instance._score;
            StartCoroutine(FadeInRoutine());
            endText.text = "Killed Ghost : " + score.ToString();
        }
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ Material ï¿½Ó°ï¿½ ï¿½ï¿½È­
    /// </summary>
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("ï¿½ï¿½ï¿½ï¿½ï¿½Ñ´ï¿½.");
            //Debug.Log(_renderers[i].material.name);
        }
        StartCoroutine(ResetMaterialAfterDelay(1.7f));
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ ï¿½Ş°ï¿½ Material ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        //// ï¿½Ú±ï¿½ ï¿½Ú½Å¿ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½
        if (other.transform.root.name == gameObject.name) return;

        // if (other.tag == "Monster")
        //     HitChangeMaterials();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "MeleeItem")
        {
            nearMeleeObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "MeleeItem")
        {
            nearMeleeObject = null;
        }
    }

    public void GetMeleeItem()
    {
        meleeItemName = nearMeleeObject.name;
        _weaponManager_S.PickUp(meleeItemName);
        Destroy(nearMeleeObject);
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // ï¿½Ö´Ï¸ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ó¼ï¿½ ï¿½ï¿½Ã¼ï¿½Ï°ï¿½ ï¿½ï¿½ï¿½Ù°ï¿½ ï¿½Ñ¾ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }

    // ê²Œì„ ?˜¤ë²? ?™”ë©? ?„œ?„œ?ˆ ?‚˜????‚˜ê²? ?•˜ê¸?
    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 1.0f;
        Color color = fadeImage.color;
        color.a = 0.0f; // ?‹œ?‘ ?•Œ?ŒŒ ê°? (?™„? „?ˆ ?ˆ¬ëª?)

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration); // ?ï¿½íŒŒ ê°’ì„ 1?ï¿½ì„œ 0?ï¿½ë¡œ ?ï¿½ì„œ??ë³?ï¿??
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1.0f; // ìµœì¢… ?•Œ?ŒŒ ê°? (?™„? „?ˆ ë¶ˆíˆ¬ëª?)
        fadeImage.color = color;
    }
}
