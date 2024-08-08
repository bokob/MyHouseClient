using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_S : MonoBehaviour
{
    public static GameManager_S _instance;
    public event EventHandler OnVariableChanged;
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
}
