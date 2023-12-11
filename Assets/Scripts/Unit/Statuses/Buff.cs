public class Buff : StatusEffect
{
    public override StatusEffectType Type => StatusEffectType.Buff;

    public override void ApplyEffect(Unit unit)
    {
        // Implement logic to enhance unit abilities
    }

    public override void RemoveEffect(Unit unit)
    {
        // Implement logic to revert the enhancements
    }
}
