using System;

[Flags]
public enum RollModifierType
{
    None = 0,
    PhysicalAttackHit = 1 << 0,
    ShoveThrow = 1 << 1,
    HealAmount = 1 << 2,
    FavorAttrition = 1 << 3,
    Dodge = 1 << 4,
    Hide = 1 << 5,
    Perception = 1 << 6,
    TerrainManipulation = 1 << 7,
    MagicAttackHit = 1 << 8,
    Flair = 1 << 9,
    IndividualFavor = 1 << 10,
    BuffHit = 1 << 11
}
