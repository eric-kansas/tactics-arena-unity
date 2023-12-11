using UnityEngine;

public class DamageOverTime : Debuff
{
    private int damagePerTick;
    private int durationInTurns;
    private int elapsedTurns = 0;

    public DamageOverTime(int damagePerTick, int durationInTurns)
    {
        this.damagePerTick = damagePerTick;
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
            unit.Damage(damagePerTick - ModifiersCalculator.HealModifer(unit));
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
        // Cleanup or reset any changes made by this debuff
        // For instance, if this debuff also slowed the unit, you would reset their speed here
    }
}
