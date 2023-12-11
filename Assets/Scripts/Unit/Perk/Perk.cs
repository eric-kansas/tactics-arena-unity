using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Perk : MonoBehaviour, IModifier
{
    public string Name;
    public string Description;

    public bool TryApplyEffect(GameEvent gameEvent, Unit unit, Unit attacker)
    {
        if (MeetsConditions(gameEvent, unit, attacker))
        {
            ApplyEffect(unit);
            return true;
        }
        return false;
    }

    public virtual bool MeetsConditions(GameEvent gameEvent, Unit unit, Unit attacker) { return false; }

    public virtual void ApplyEffect(Unit unit) { }

    public virtual void ApplyEffect(Unit unit, Unit attacker) { }


    public virtual int GetStatModifier(StatModifierType modifierType)
    {
        return 0;
    }

    public virtual int GetRollStatModifier(RollModifierType modifierType)
    {
        return 0;
    }

    public virtual int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        return 0;
    }
}