public class EagleEyePerk : Perk
{

    public override int GetDerivedStatModifier(DerivedStatModifierType modifierType)
    {
        if (modifierType == DerivedStatModifierType.SightDistance)
        {
            return 2;
        }
        return 0;
    }

    public override int GetRollStatModifier(RollModifierType modifierType)
    {
        if (modifierType == RollModifierType.Perception)
        {
            return 2;
        }
        return 0;
    }

}