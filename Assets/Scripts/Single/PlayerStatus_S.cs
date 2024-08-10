using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour
{
    #region »óÅÂ ¹× ´É·ÂÄ¡
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // Ã¼·Â
    [field: SerializeField] public float Sp { get; set; } = 100;    // ½ºÅ×¹Ì³ª
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // ÃÖ´ë Ã¼·Â
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // ÃÖ´ë ½ºÅ×¹Ì³ª
    [field: SerializeField] public float Defence { get; private set; } = 1; // ¹æ¾î·Â
    #endregion

    #region ¾Ö´Ï¸ÞÀÌ¼Ç ¹× ÇÇÇØ
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

        // ·»´õ °¡Á®¿À±â
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("¿Ö »öÀÌ ³Î?");
            }
        }
    }

    void Update()
    {
        Dead();
        //TransformIntoHouseowner();
        if (_dead)
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
    /// ¿ªÇÒ ÃÊ±âÈ­
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        È£½ºÆ®¸é, HouseownerÀ¸·Î ÇÏ°í, Å¬¶óÀÌ¾ðÆ®¸é Robber

        ½Ì±ÛÀº ÁýÁÖÀÎ¸¸
         */
        Role = Define.Role.Houseowner;
    }



    /// <summary>
    /// µ¥¹ÌÁö ÀÔ±â
    /// </summary>
    /// <param name="attack"> °¡ÇÒ °ø°Ý·Â </param>
    public void TakedDamage(int attack)
    {
        // ÇÇÇØ°¡ À½¼ö¶ó¸é È¸º¹µÇ´Â Çö»óÀÌ ÀÏ¾î³ª¹Ç·Î ÇÇÇØÀÇ °ªÀ» 0ÀÌ»óÀ¸·Î µÇ°Ô²û ¼³Á¤
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;

        Debug.Log(gameObject.name + "(ÀÌ)°¡ " + damage + " ¸¸Å­ ÇÇÇØ¸¦ ÀÔ¾úÀ½!");
        Debug.Log("³²Àº Ã¼·Â: " + Hp);
    }

    /// <summary>
    /// ÃÖ´ë Ã¼·ÂÀÇ 0.2¸¸Å­ È¸º¹
    /// </summary>
    public void Heal()
    {
        // ÇöÀç Ã¼·ÂÀÌ ÃÖ´ë Ã¼·Âº¸´Ù ÀÛÀ» ¶§¸¸ È¸º¹ Àû¿ë
        if (Hp < MaxHp)
        {
            // È¸º¹·®
            float healAmount = MaxHp * 0.2f;

            // È¸º¹·®°ú ÇöÀç Ã¼·Â°úÀÇ ÇÕÀÌ ÃÖ´ë Ã¼·ÂÀ» ³ÑÁö ¾Êµµ·Ï Á¶Àý
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("ÀÌÀü Ã¼·Â" + Hp);
            // Ã¼·Â È¸º¹
            Hp += healedAmount;
            Debug.Log("Ã¼·ÂÀ» " + healedAmount + "¸¸Å­ È¸º¹!");
            Debug.Log("ÇöÀç Ã¼·Â: " + Hp);
        }
        else
        {
            Debug.Log("ÃÖ´ë Ã¼·Â. È¸º¹ÇÒ ÇÊ¿ä ¾øÀ½.");
        }
    }

    /// <summary>
    /// ÃÖ´ë ½ºÅ×¹Ì³ª±îÁö ÀüºÎ È¸º¹
    /// </summary>
    public void SpUp()
    {
        // ÇöÀç ½ºÅ×¹Ì³ª°¡ ÃÖ´ë ½ºÅ×¹Ì³ªº¸´Ù ÀÛÀ» ¶§¸¸ È¸º¹ Àû¿ë
        if (Sp < MaxSp)
        {
            // È¸º¹·®°ú ÇöÀç ½ºÅ×¹Ì³ª¿ÍÀÇ ÇÕÀÌ ÃÖ´ë ½ºÅ×¹Ì³ª¸¦ ³ÑÁö ¾Êµµ·Ï Á¶Àý
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("ÀÌÀü ½ºÅ×¹Ì³ª" + Sp);
            // ½ºÅ×¹Ì³ª È¸º¹
            Sp += healedAmount;
            Debug.Log("ÀüºÎ È¸º¹! ÇöÀç Sp: " + Sp);
        }
        else
        {
            Debug.Log("ÃÖ´ë Sp. È¸º¹ÇÒ ÇÊ¿ä ¾øÀ½.");
        }
    }

    /// <summary>
    /// ½ºÅ×¹Ì³ª Â÷¿À¸£±â
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ½ºÅ×¹Ì³ª ±ðÀÌ±â
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// Á¡ÇÁ½Ã, ½ºÅ×¹Ì³ª °¨¼Ò
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// ¹æ¾î·Â Áõ°¡
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// »ç¸Á
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            score = GameManager_S._instance._score;
            endUI.SetActive(true);
            StartCoroutine(FadeInRoutine());
            endText.text = "Killed Ghost : " + score.ToString();
            _animator.SetTrigger("setDie");
            _dead = true;
            Role = Define.Role.None; // ½ÃÃ¼
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    /// <summary>
    /// °ÔÀÓ ³¡³»±â
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
    /// ÇÇÇØ ¹ÞÀ¸¸é Material ºÓ°Ô º¯È­
    /// </summary>
    public void HitChangeMaterials()
    {
        // ÅÂ±×°¡ ¹«±â ¶Ç´Â ¸ó½ºÅÍ

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("»öº¯ÇÑ´Ù.");
            //Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"ÇÃ·¹ÀÌ¾î°¡ {other.transform.root.name}¿¡°Ô °ø°Ý ¹ÞÀ½!");
        Debug.Log("°ø°Ý¹ÞÀº ÃøÀÇ Ã¼·Â:" + Hp);
    }

    /// <summary>
    /// ÇÇÇØ ¹Þ°í Material ¿ø·¡´ë·Î º¹±¸
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
        //// ÀÚ±â ÀÚ½Å¿¡°Ô ´êÀº °æ¿ì ¹«½Ã
        if (other.transform.root.name == gameObject.name) return;

        if (other.tag == "Melee" || other.tag == "Gun" || other.tag == "Monster")
            HitChangeMaterials();
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

        // ¾Ö´Ï¸ÞÀÌÅÍ ¼Ó¼º ±³Ã¼ÇÏ°í ²°´Ù°¡ ÄÑ¾ß µ¿ÀÛÇÔ
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 1.0f;
        Color color = fadeImage.color;
        color.a = 0.0f; // ?œìž‘ ?ŒíŒŒ ê°?(?„ì „??ë¶ˆíˆ¬ëª?

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration); // ?ŒíŒŒ ê°’ì„ 1?ì„œ 0?¼ë¡œ ?œì„œ??ë³€ê²?
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1.0f; // ìµœì¢… ?ŒíŒŒ ê°?(?„ì „???¬ëª…)
        fadeImage.color = color;
    }
}
