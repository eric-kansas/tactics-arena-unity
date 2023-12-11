using System;

[Flags]
public enum StatModifierType
{
    None = 0,
    Might = 1 << 0,
    Endurance = 1 << 1,
    Agility = 1 << 2,
    Perception = 1 << 3,
    Intelligence = 1 << 4,
    Charisma = 1 << 5
}
