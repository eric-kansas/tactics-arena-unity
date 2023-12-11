
public class Debuff : StatusEffect
{
    public override StatusEffectType Type => StatusEffectType.Debuff;

    public override void ApplyEffect(Unit unit)
    {
        // Implement logic to reduce or impair unit abilities
    }

    public override void RemoveEffect(Unit unit)
    {
        // Implement logic to revert the impairments
    }
}
