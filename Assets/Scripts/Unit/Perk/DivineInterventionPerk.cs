using UnityEngine;

public class DivineInterventionPerk : Perk
{
    public override bool CanPreventDeath(Unit unit)
    {
        // Logic to determine if this perk can prevent death (e.g., based on a probability)
        return Random.value < 0.25f; // 25% chance to prevent death
    }

    public override void ApplyEffect(Unit unit)
    {
        // Logic for applying the perk's effect, such as healing
        unit.Heal(5); // Heals a small amount
    }
}