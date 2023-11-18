using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFavor : MonoBehaviour
{

    public event EventHandler OnFull;
    public event EventHandler OnRemoved;
    public event EventHandler OnAdded;

    [SerializeField] private int favor = 0;
    [SerializeField] private int favorMax = 100;

    public void Add(int favorAmount)
    {
        favor += favorAmount;

        if (favor > favorMax)
        {
            favor = favorMax;
        }
        OnAdded?.Invoke(this, EventArgs.Empty);

        if (favor == favorMax)
        {
            OnFull?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Remove(int damageAmount)
    {
        favor -= damageAmount;

        if (favor < 0)
        {
            favor = 0;
        }

        OnRemoved?.Invoke(this, EventArgs.Empty);
    }

    public float GetFavorNormalized()
    {
        return (float)favor / favorMax;
    }

    public int GetMaxFavor()
    {
        return favorMax;
    }

}
