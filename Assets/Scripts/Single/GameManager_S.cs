using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager_S : MonoBehaviour
{    
    public static GameManager_S _instance;
    public int _monsterCount = 0;
    public int _score = 0;
    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
        }
    }

    void Awake()
    {
        _instance = this;
        _score = 0;
    }

    // ���⸦ �ֿ��� �� ȣ��Ǵ� �޼���
    public void PickUpWeapon(string weaponName)
    {
        WeaponData weapon = weaponManager_S.GetWeaponByName(weaponName);
        if (weapon != null)
        {
            Debug.Log($"Picked up {weapon.Name}. Attack: {weapon.Attack}, Rate: {weapon.Rate}");
            _currentWeapon = weapon.Name;
            _currentAttack = weapon.Attack;
            _currentRate = weapon.Rate;
        }
        else
        {
            Debug.LogWarning("Weapon not found!");
        }
    }
}

