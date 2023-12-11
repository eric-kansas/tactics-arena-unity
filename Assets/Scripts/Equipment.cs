using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Game/Equipment")]
public class Equipment : ScriptableObject
{
    public enum EquipmentType { Helm, ChestPlate, Weapon, Ring }
    public EquipmentType type;

    [SerializeField] private string itemName; // Name of the equipment
    [SerializeField] private int physicalAttackBonus; // Bonus to attack when equipped
    [SerializeField] private int magicalAttackBonus;
    [SerializeField] private int armorBonus;  // Defensive bonus when equipped
    [SerializeField] private int durability;  // Durability of the equipment
    [SerializeField] private float weight;    // Weight of the equipment

    [SerializeField] private string description; // Description of the item
    [SerializeField] private Sprite icon;        // Icon for the item
    [SerializeField] private List<AbilityData> abilities;

    public string ItemName => itemName;
    public int PhysicalAttackBonus => physicalAttackBonus;
    public int MagicalAttackBonus => magicalAttackBonus;
    public int ArmorBonus => armorBonus;
    public int Durability => durability;
    public float Weight => weight;
    public string Description => description;
    public Sprite Icon => icon;
    public List<AbilityData> Abilities => abilities;
}
