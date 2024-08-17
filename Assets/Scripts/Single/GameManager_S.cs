using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

// 
public class WeaponData
{
    public string Name;
    public Define.Type Type;
    public int Attack;
    public float Rate;
    public float Range;
}

public class GameManager_S : MonoBehaviour
{    
    public static GameManager_S _instance;
    public int _monsterCount = 0;
    public int _score = 0;
    public int Score { get { return _score; } set { _score = value;} }

    [Header("Weapon Status")]
    public List<WeaponData> weaponStatusList;

    void Awake()
    {
        _instance = this;

        LoadWeaponData();
    }

    // 무기 정보 로드
    void LoadWeaponData()
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Single/Weapon/weapondata.json");

        if (File.Exists(filePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                weaponStatusList = JsonConvert.DeserializeObject<List<WeaponData>>(jsonContent);

                if (weaponStatusList != null)
                {
                    Debug.Log("Weapon data loaded successfully.");

                    // ???? ????? ??
                    if (weaponStatusList.Count == 0)
                    {
                        Debug.LogWarning("Weapon list is empty.");
                    }
                    else
                    {
                        foreach (var weaponStatus in weaponStatusList)
                        {
                            Debug.Log($"Weapon: {weaponStatus.Name}, Attack: {weaponStatus.Attack}, Rate: {weaponStatus.Rate}");
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load weapon data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Weapon data file not found.");
        }
    }

    // 무기 이름으로 무기 정보 탐색

    public WeaponData GetWeaponStatusByName(string weaponName)
    {
        return weaponStatusList.Find(weapon => weapon.Name == weaponName);
    }
}