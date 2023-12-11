using System;
using UnityEngine;

[Serializable]
public class Gear
{
    [SerializeField] private Equipment helm;
    [SerializeField] private Equipment chestPlate;
    [SerializeField] private Equipment weapon;
    [SerializeField] private Equipment ring;

    // Getters for each equipment slot
    public Equipment Helm => helm;
    public Equipment ChestPlate => chestPlate;
    public Equipment Weapon => weapon;
    public Equipment Ring => ring;

    // Method to calculate total armor bonus
    internal int GetTotalArmorBonus()
    {
        return GetEquipmentArmorBonus(helm) + GetEquipmentArmorBonus(chestPlate) +
               GetEquipmentArmorBonus(weapon) + GetEquipmentArmorBonus(ring);
    }

    // Method to calculate total attack bonus
    internal int GetTotalPhysicalAttackBonus()
    {
        return GetEquipmentPhysicalAttackBonus(helm) + GetEquipmentPhysicalAttackBonus(chestPlate) +
               GetEquipmentPhysicalAttackBonus(weapon) + GetEquipmentPhysicalAttackBonus(ring);
    }

    internal int GetTotalMagicalAttackBonus()
    {
        return GetEquipmentMagicalAttackBonus(helm) + GetEquipmentMagicalAttackBonus(chestPlate) +
               GetEquipmentMagicalAttackBonus(weapon) + GetEquipmentMagicalAttackBonus(ring);
    }

    // Utility methods for calculating bonuses from an equipment piece
    private int GetEquipmentArmorBonus(Equipment equipment)
    {
        return equipment != null ? equipment.ArmorBonus : 0;
    }

    private int GetEquipmentPhysicalAttackBonus(Equipment equipment)
    {
        return equipment != null ? equipment.PhysicalAttackBonus : 0;
    }

    private int GetEquipmentMagicalAttackBonus(Equipment equipment)
    {
        return equipment != null ? equipment.MagicalAttackBonus : 0;
    }
}
