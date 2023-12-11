using UnityEngine;

public class SlowedStatus : Debuff
{
    private int damagePerTick;
    private int durationInTurns;
    private int elapsedTurns = 0;
    private bool active;

    public SlowedStatus(int durationInTurns)
    {
        this.durationInTurns = durationInTurns;
    }

    public override bool MeetsConditions(GameEvent gameEvent, Unit unit, Unit attacker)
    {
        if (gameEvent == GameEvent.TurnStart)
        {
            return true;
        }
        return false;
    }


    public override void ApplyEffect(Unit unit)
    {
        // Apply damage if the debuff is still active
        if (elapsedTurns < durationInTurns)
        {
            active = true;
            elapsedTurns += 1;
        }
        else
        {
            // Optional: Automatically remove the effect once the duration is over
            RemoveEffect(unit);
        }
    }

    public override void RemoveEffect(Unit unit)
    {
        active = false;
        elapsedTurns = 0;
        unit.RemoveStatus(this);
    }

    public override int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        if (active && modifierType == DerivedStatModifierType.MoveSpeed)
        {
            return -2;
        }
        return 0;
    }

    public override int GetRollStatModifier(RollModifierType modifierType)
    {
        if (active && modifierType == RollModifierType.Dodge)
        {
            return -2;
        }
        return 0;
    }

}
