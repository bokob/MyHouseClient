using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WeaponData : MonoBehaviour
{
    public string Name;
    public float Attack;
    public float Rate;

    #region Item Json 처리
    public WeaponData weaponData;
    #endregion
    void SaveWeaponDataToString()
    {
        string jsonData = JsonUtility.ToJson(weaponData, true);
        string folderPath = Path.Combine(Application.dataPath, "Item");
        string path = Path.Combine(folderPath, "weaponData.json");

        // JSON 데이터를 파일로 저장
        File.WriteAllText(path, jsonData);

        Debug.Log("Weapon data saved to " + path);
    }
}
