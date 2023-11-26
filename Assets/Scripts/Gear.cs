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
    internal int GetTotalAttackBonus()
    {
        return GetEquipmentAttackBonus(helm) + GetEquipmentAttackBonus(chestPlate) +
               GetEquipmentAttackBonus(weapon) + GetEquipmentAttackBonus(ring);
    }

    // Utility methods for calculating bonuses from an equipment piece
    private int GetEquipmentArmorBonus(Equipment equipment)
    {
        return equipment != null ? equipment.ArmorBonus : 0;
    }

    private int GetEquipmentAttackBonus(Equipment equipment)
    {
        return equipment != null ? equipment.AttackBonus : 0;
    }
}
