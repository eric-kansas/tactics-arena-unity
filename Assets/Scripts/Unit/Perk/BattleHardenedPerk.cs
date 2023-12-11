public class BattleHardenedPerk : Perk
{

    public override int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        if (modifierType == DerivedStatModifierType.PhysicalArmor)
        {
            return 1;
        }
        return 0;
    }

}