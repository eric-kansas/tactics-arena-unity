public class EnhancedHealingPerk : Perk
{
    public override int GetRollStatModifier(RollModifierType modifierType)
    {
        if (modifierType == RollModifierType.HealAmount)
        {
            return 3;
        }
        return 0;
    }

}