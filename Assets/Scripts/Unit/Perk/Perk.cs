using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Perk : MonoBehaviour
{
    public string Name;
    public string Description;

    public virtual void ApplyEffect(Unit unit) { }


    public virtual void ApplyEffect(Unit unit, Unit attacker)
    {

    }


    public virtual void ResetPerk() { }


    public virtual int GetArmorClassBonus() { return 0; }

    public virtual bool CanPreventDeath(Unit unit) { return false; }


}