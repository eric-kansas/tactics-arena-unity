using UnityEngine;

public class DivineInterventionPerk : Perk
{
    public override bool MeetsConditions(GameEvent gameEvent, Unit unit, Unit attacker)
    {
        if (GameEvent.UnitEnergyDepleted == gameEvent)
        {
            // Logic to determine if this perk can prevent death (e.g., based on a probability)
            return Random.value < 0.25f; // 25% chance to prevent death
        }
        return false;
    }

    public override void ApplyEffect(Unit unit)
    {
        unit.Heal(ModifiersCalculator.HealModifer(unit)); // Heals a small amount
    }
}