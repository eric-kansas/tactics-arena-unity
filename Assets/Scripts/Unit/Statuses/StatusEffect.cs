public abstract class StatusEffect : IModifier
{
    public string Name { get; protected set; }
    public float Duration { get; protected set; }
    public abstract StatusEffectType Type { get; }


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

    public abstract void ApplyEffect(Unit unit);

    public abstract void RemoveEffect(Unit unit);


    public virtual int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        return 0;
    }

    public virtual int GetRollStatModifier(RollModifierType modifierType)
    {
        return 0;
    }

    public virtual int GetStatModifier(StatModifierType modifierType)
    {
        return 0;
    }

}