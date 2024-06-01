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
    public enum Type
    {
        Melee,
        Range,
    }

    public enum Item
    {
        Heart,
        Energy,
        Weapon,
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
}
