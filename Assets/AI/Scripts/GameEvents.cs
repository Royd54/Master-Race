using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    private void Awake()
    {
        instance = this;
    }

    public event Action onEnemyDeath;
    public void EnemyDeath()
    {
        if (onEnemyDeath != null)
        {
            onEnemyDeath();
        }
    }

    public event Action onSlomo;
    public void Slomo()
    {
        if (onSlomo != null)
        {
            onSlomo();
        }
    }

    public event Action onLightstrigger;
    public void LightsTrigger()
    {
        if (onLightstrigger != null)
        {
            onLightstrigger();
        }
    }
}
