using System;

[Flags]
public enum DerivedStatModifierType
{
    None = 0,
    PushThrowDistance = 1 << 0,
    MaxEnergy = 1 << 1,
    MoveSpeed = 1 << 2,
    SightDistance = 1 << 3,
    XPGain = 1 << 4,
    MaxFavor = 1 << 5,
    PhysicalArmor = 1 << 6,
    MagicArmor = 1 << 7
}
