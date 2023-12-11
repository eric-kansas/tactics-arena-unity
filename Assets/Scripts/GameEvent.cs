public enum GameEvent
{
    // Match Events
    MatchStart,
    MatchEnd,

    // Turn Events
    TurnStart,
    TurnEnd,

    // Team Events
    TeamVisibilityChanged,
    TeamEnergyChanged,
    TeamFavorChanged,
    TeamTerritoryScoreChanged,

    // Unit Events
    UnitSpawn,
    UnitEnergyChanged,
    UnitEnergyDepleted,
    UnitFavorChanged,
    UnitTerritoryChanged,
    UnitEnteredVision,
    UnitChangedGridPosition,
    UnitChangedElevation,
    UnitGainsCover,
    UnitPerceived,
    UnitShoved,
    UnitXPGained,


    // Grid Events
    GridElevationChanged,
    GridTypeChanged,
    TerritoryOwnerChanged,

    // Attack Events
    MeleeAttack,
    RangedAttack,
    MagicAttack,
    BuffAttack,
    ShoveAttack,
    CounterAttack,
    OpportunityAttack,
    AttackHit,
    AttackMiss,

    // Buff and Debuff Events
    BuffHit,
    UnitApplyingBuffs,
    UnitRemovingDebuffs,
    StatusEffectExpiration,

    // Roll Events
    FavorRoll,
    HealRoll,
    HideRoll,
    PerceptionRoll,
    FavorAttritionRoll,
    PhysicalDamageRoll,
    MagicDamageRoll,

    // Outcome Events
    CriticalHit,
    UnitDodge,
    UnitHide
}