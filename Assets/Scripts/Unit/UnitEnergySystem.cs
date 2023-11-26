using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnergySystem : MonoBehaviour
{

    public event EventHandler OnOutOfEnergy;
    public event EventHandler OnDamaged;
    public event EventHandler OnHealed;

    [SerializeField] private int energy = 100;
    [SerializeField] private int energyMax;


    private void Awake()
    {
        energyMax = energy;
    }

    public void Heal(int healAmount)
    {
        energy += healAmount;

        if (energy > energyMax)
        {
            energy = energyMax;
        }
        OnHealed?.Invoke(this, EventArgs.Empty);
    }

    public void Damage(int damageAmount)
    {
        energy -= damageAmount;

        if (energy < 0)
        {
            energy = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);

        if (energy == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnOutOfEnergy?.Invoke(this, EventArgs.Empty);
    }

    public float GetEnergyNormalized()
    {
        return (float)energy / energyMax;
    }

    public int GetEnergy()
    {
        return energy;
    }

    public int GetMaxEnergy()
    {
        return energyMax;
    }

    public void SetMaxHealth(int max)
    {
        energyMax = max;
        if (energy > max)
        {
            energy = max;
        }
    }

}
