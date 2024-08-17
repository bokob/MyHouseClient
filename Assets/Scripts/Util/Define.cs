using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Role
    {
        None,
        Houseowner,
        Robber
    }
    public enum MonsterState
    {
        None,
        Idle,
        Chase,
        Attack,
        Patrol,
        Hit,
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
