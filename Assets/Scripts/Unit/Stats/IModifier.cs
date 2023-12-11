
public interface IModifier
{
    int GetStatModifier(StatModifierType modifierType);
    int GetRollStatModifier(RollModifierType modifierType);
    int GetDerivedStatModifier(DerivedStatModifierType modifierType);
}
