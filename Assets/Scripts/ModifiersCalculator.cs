using System;
using UnityEngine;

public class ModifiersCalculator
{
    public static int PhysicalHitModifier(Unit unit, Unit target)
    {
        int statModifer = unit.GetStats().Might;
        int perksStatBonus = CalculatePerksStatBonus(unit, StatModifierType.Might);
        int statModiferTotal = statModifer + perksStatBonus;

        int gearModifer = unit.GetPlayerData().GetGear().GetTotalPhysicalAttackBonus();
        int perkRollStatBonus = CalculatePerkRollStatBonus(unit, RollModifierType.PhysicalAttackHit);
        int statusesBonus = CalculateStatusesRollStatBonus(unit, RollModifierType.PhysicalAttackHit);
        int terrainModifer = CalculateTerrainModifier(unit, target);

        int totalModifier = statModiferTotal + gearModifer + perkRollStatBonus + statusesBonus + terrainModifer;

        Debug.Log($"Physical Attack Modifier:\n" +
                  $"- Stat: Base[{statModifer}] + Perks[{perksStatBonus}] = {statModiferTotal}\n" +
                  $"- Gear: {gearModifer}\n" +
                  $"- Perks Physical Attack: {perkRollStatBonus}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"- Terrain: {terrainModifer}\n" +
                  $"Total Modifier: {totalModifier}");

        return totalModifier;
    }

    public static int MagicAttackModifier(Unit unit, Unit target)
    {
        int statModifer = unit.GetStats().Intelligence;
        int perkStatBonus = CalculatePerksStatBonus(unit, StatModifierType.Intelligence);
        int statModiferTotal = statModifer + perkStatBonus;

        int gearModifer = unit.GetPlayerData().GetGear().GetTotalMagicalAttackBonus();
        int perkRollStatBonus = CalculatePerkRollStatBonus(unit, RollModifierType.MagicAttackHit);
        int statusesBonus = CalculateStatusesRollStatBonus(unit, RollModifierType.MagicAttackHit);
        int terrainModifer = CalculateTerrainModifier(unit, target);

        int totalModifier = statModiferTotal + gearModifer + perkRollStatBonus + statusesBonus + terrainModifer;

        Debug.Log($"Magic Attack Modifier:\n" +
                  $"- Stat: Base[{statModifer}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Gear: {gearModifer}\n" +
                  $"- Perks Magic Attack: {perkRollStatBonus}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"- Terrain: {terrainModifer}\n" +
                  $"Total Modifier: {totalModifier}");

        return totalModifier;
    }


    private static int CalculatePerksStatBonus(Unit unit, StatModifierType modifierType)
    {
        int totalBonus = 0;
        foreach (Perk perk in unit.GetPerks())
        {
            totalBonus += perk.GetStatModifier(modifierType);
        }
        return totalBonus;
    }

    private static int CalculatePerkDerivedStatBonus(Unit unit, DerivedStatModifierType modifierType)
    {
        int totalBonus = 0;
        foreach (Perk perk in unit.GetPerks())
        {
            totalBonus += perk.GetDerivedStatModifier(modifierType);
        }
        return totalBonus;
    }


    private static int CalculatePerkRollStatBonus(Unit unit, RollModifierType modifierType)
    {
        int totalBonus = 0;
        foreach (Perk perk in unit.GetPerks())
        {
            totalBonus += perk.GetRollStatModifier(modifierType);
        }
        return totalBonus;
    }

    private static int CalculateStatusesRollStatBonus(Unit unit, RollModifierType modifierType)
    {
        int totalBonus = 0;
        foreach (StatusEffect statusEffect in unit.GetStatusEffects())
        {
            totalBonus += statusEffect.GetRollStatModifier(modifierType);
        }
        return totalBonus;
    }

    private static int CalculateStatusesDerivedStatBonus(Unit unit, DerivedStatModifierType modifierType)
    {
        int totalBonus = 0;
        foreach (StatusEffect statusEffect in unit.GetStatusEffects())
        {
            totalBonus += statusEffect.GetDerivedStatModifier(modifierType);
        }
        return totalBonus;
    }

    public static int CalculateTerrainModifier(Unit attacker, Unit target)
    {
        int modifier = 0;
        int coverPenalty = 2; // Example cover penalty value

        GridPosition attackerPos = attacker.GetGridPosition();
        GridPosition targetPos = target.GetGridPosition();

        // Elevation advantage
        int elevationDifference = LevelGrid.Instance.GetElevationAtGridPosition(attackerPos) - LevelGrid.Instance.GetElevationAtGridPosition(targetPos);
        if (elevationDifference > 0)
        {
            modifier += elevationDifference / 2;
        }

        // Check target cover
        Direction attackDirection = CalculateAttackDirection(attackerPos, targetPos);
        CoverLevel coverLevel = CoverSystem.Instance.GetCoverAtPosition(targetPos, attackDirection);
        if (coverLevel != CoverLevel.None)
        {
            modifier -= coverPenalty;
        }

        // Check terrain type
        TerrainType terrainType = LevelGrid.Instance.GetTerrainTypeAtGridPosition(targetPos);
        modifier += GetTerrainTypeModifier(terrainType);

        return modifier;
    }

    private static Direction CalculateAttackDirection(GridPosition attackerPos, GridPosition targetPos)
    {
        int deltaX = targetPos.x - attackerPos.x;
        int deltaZ = targetPos.z - attackerPos.z;

        // Normalize the deltas to -1, 0, or 1
        int signX = Math.Sign(deltaX);
        int signZ = Math.Sign(deltaZ);

        if (signX == 0 && signZ == 1)
            return Direction.North;
        if (signX == 1 && signZ == 0)
            return Direction.East;
        if (signX == 0 && signZ == -1)
            return Direction.South;
        if (signX == -1 && signZ == 0)
            return Direction.West;

        // For intercardinal directions
        if (signX == 1 && signZ == 1)
            return Direction.NorthEast;
        if (signX == 1 && signZ == -1)
            return Direction.SouthEast;
        if (signX == -1 && signZ == -1)
            return Direction.SouthWest;
        if (signX == -1 && signZ == 1)
            return Direction.NorthWest;

        // Default case, can be used to handle unexpected scenarios
        return Direction.North; // Assuming 'None' is a valid value in your Direction enum
    }

    private static int GetTerrainTypeModifier(TerrainType terrainType)
    {
        return 0;
    }

    public static int PushModifer(Unit unit)
    {
        return unit.GetStats().Might;
    }

    public static int HealModifer(Unit unit)
    {
        return unit.GetStats().Endurance;
    }

    public static int FavorAttritionModifer(Unit unit)
    {
        return -unit.GetStats().Endurance;
    }

    public static int DodgeModifer(Unit unit)
    {
        return unit.GetStats().Agility;
    }

    public static int HideModifer(Unit unit)
    {
        return unit.GetStats().Agility;
    }

    public static int PerceptionModifer(Unit unit)
    {
        return unit.GetStats().Perception;
    }

    public static int FlairModifier(Unit unit)
    {
        return unit.GetStats().Intelligence;
    }

    public static int CommandModifier(Unit unit)
    {
        return unit.GetStats().Intelligence;
    }

    public static int FavorModifier(Unit unit)
    {
        return unit.GetStats().Charisma;
    }

    public static int InspireModifer(Unit unit)
    {
        return unit.GetStats().Charisma;
    }
    internal static int PushStrength(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetPushStrength();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.PushThrowDistance);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.PushThrowDistance);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Push Strength Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");

        return totalModifier;
    }


    internal static int MaxEnergy(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetMaxEnergy();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.MaxEnergy);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.MaxEnergy);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Max Move Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");

        return totalModifier;
    }

    internal static int MoveSpeed(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetMoveSpeed();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.MoveSpeed);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.MoveSpeed);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Move Speed Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }

    internal static int SightDistance(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetMoveSpeed();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.SightDistance);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.SightDistance);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Sight Distance Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }

    internal static int XPGain(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetXPGain();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.XPGain);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.XPGain);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"XP gain Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }
    internal static int MaxFavor(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetMaxFavor();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.MaxFavor);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.MaxFavor);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"MaxFavor Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }
    internal static int PhysicalArmor(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetPhysicalArmour();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.PhysicalArmor);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.PhysicalArmor);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Physical Armor Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }
    internal static int MagicArmor(Unit unit)
    {
        int baseModeSpeed = unit.GetStats().GetMagicArmor();
        int perkStatBonus = CalculatePerkDerivedStatBonus(unit, DerivedStatModifierType.MagicArmor);
        int statModiferTotal = baseModeSpeed + perkStatBonus;

        // TODO Gear

        int statusesBonus = CalculateStatusesDerivedStatBonus(unit, DerivedStatModifierType.MagicArmor);

        int totalModifier = statModiferTotal + statusesBonus;

        Debug.Log($"Magic Armor Modifier:\n" +
                  $"- Stat: Base[{baseModeSpeed}] + Perks[{perkStatBonus}] = {statModiferTotal}\n" +
                  $"- Statuses: {statusesBonus}\n" +
                  $"Total Modifier: {totalModifier}");


        return totalModifier;
    }
}
