using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public static string _sceneName = "TitleScene";

    public enum Role
    {
        None,
        Houseowner,
        Robber,
        Monster
    }

    public enum View
    {
        None,
        Quater,
        Third,
    }
    public enum Type
    {
        Melee,
        Range,
    }

    public enum Item
    {
        None,
        Status,
        Weapon,
    }

    public enum StatusItem
    {
        Heart,
        Energy,
    }

    public enum WeaponItem
    {
        Axe,
        Bat,
        HockeyStick,
    }
}
