using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MonsterController_S : MonoBehaviour
{
    public event EventHandler OnVariableChanged;
    public int _monsterCount = 0;
    public int _score = 0;
    public Gun_S _guns;
    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            OnVariableChanged?.Invoke(this, new EventArgs());
        }
    }

    void Start()
    {
        _score = 0;
        OnVariableChanged += HandleVariableChanged;
    }

    void HandleVariableChanged(object sender, EventArgs e)
    {
        _guns._totalBulletCount += 10;
        Debug.Log("Variable changed! Count is now: " + _score);
    }

    public int GetCount()
    {
        return _score;
    }
}
