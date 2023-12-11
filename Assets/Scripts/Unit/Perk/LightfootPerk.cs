public class LightfootPerk : Perk
{

    public override int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        if (modifierType == DerivedStatModifierType.MoveSpeed)
        {
            return 1;
        }
        return 0;
    }

    public override int GetRollStatModifier(RollModifierType modifierType)
    {
        if (modifierType == RollModifierType.Dodge)
        {
            return 1;
        }
        return 0;
    }

}