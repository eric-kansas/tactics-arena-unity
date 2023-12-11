using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 2;

    public static event Action OnAnyActionPointsChanged;
    public static event Action<Unit> OnAnyUnitInitialized;
    public static Action<Unit> OnAnyUnitOutOfEnergy;
    public static Action<Unit> OnAnyUnitExpendFavor;

    [SerializeField] private Team team;
    [SerializeField] private LayerMask unitLayerMask; // If used
    [SerializeField] private SkinnedMeshRenderer unitRenderer;

    private Player playerData;
    private GridPosition gridPosition;
    private UnitEnergySystem unitEnergySystem;
    private UnitFavor favorSystem;
    private Perk[] basePerkArray;
    private StatusEffect[] baseStatusEffectArray;
    private BaseAction[] baseActionArray;
    private int actionPoints = ACTION_POINTS_MAX;
    private bool inArena = true;
    private bool isProne;

    private void Start()
    {
        LevelGrid.Instance.OnElevationChanged += LevelGrid_OnElevationChange;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        unitEnergySystem.OnOutOfEnergy += EnergySystem_OnOutOfEnergy;

        OnAnyUnitInitialized?.Invoke(this);
    }

    public void Setup(Team team, Player player)
    {
        unitEnergySystem = GetComponent<UnitEnergySystem>();
        favorSystem = GetComponent<UnitFavor>();
        basePerkArray = GetComponents<Perk>();
        baseActionArray = new BaseAction[0];
        baseStatusEffectArray = new StatusEffect[0];

        this.team = team;
        playerData = player;

        unitEnergySystem.SetMaxHealth(ModifiersCalculator.MaxEnergy(this));
        favorSystem.SetMaxFavor(ModifiersCalculator.MaxFavor(this));

        foreach (var abilityData in player.GetAbilities())
        {
            Type abilityType = Type.GetType(abilityData.abilityBehaviourName);
            if (abilityType != null)
            {
                gameObject.AddComponent(abilityType);
            }
            else
            {
                Debug.LogError("Ability type not found: " + abilityData.abilityBehaviourName);
            }
        }

        baseActionArray = GetComponents<BaseAction>();

        foreach (var perkData in player.GetPerks())
        {
            Type perkType = Type.GetType(perkData.perkBehaviourName);
            if (perkType != null)
            {
                gameObject.AddComponent(perkType);
            }
            else
            {
                Debug.LogError("Ability type not found: " + perkData.perkBehaviourName);
            }
        }



        unitRenderer.material = team.GetMaterial();
    }

    private void Update()
    {
        if (inArena)
        {
            GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            if (newGridPosition != gridPosition)
            {
                GridPosition oldGridPosition = gridPosition;
                gridPosition = newGridPosition;

                LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
            }
        }
    }

    public T GetAction<T>() where T : class
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T action)
            {
                return action;
            }
        }
        return null;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (actionPoints >= baseAction.GetActionPointsCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    internal bool CanTakeAction(BaseAction baseAction)
    {
        return CanSpendActionPointsToTakeAction(baseAction) && baseAction.MeetsRequirements(GetGridPosition());
    }

    internal bool CanTakeAnyActions()
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (CanTakeAction(baseAction))
            {
                return true;
            }
        }

        return false;
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;

        OnAnyActionPointsChanged?.Invoke();
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.GetCurrentTeam() == GetTeam() && inArena)
        {
            // right now restore all action points
            actionPoints = ACTION_POINTS_MAX;

            UpdateModifers(GameEvent.TurnStart);

            ApplyFavorAttrition();

            OnAnyActionPointsChanged?.Invoke();
        }
    }

    private void ApplyFavorAttrition()
    {
        System.Random random = new System.Random();
        int favorAttrition = random.Next(1, 9);
        favorAttrition += ModifiersCalculator.FavorAttritionModifer(this);
        favorSystem.Remove(favorAttrition);
    }

    public void UpdateModifers(GameEvent gameEvent)
    {
        // try update gear

        // try update and apply perks
        foreach (Perk perk in basePerkArray)
        {
            perk.TryApplyEffect(gameEvent, this, null);
        }

        // try update and apply status effects
        foreach (StatusEffect statusEffect in baseStatusEffectArray)
        {
            statusEffect.TryApplyEffect(gameEvent, this, null);
        }
    }

    private void LevelGrid_OnElevationChange(GridPosition position, int newElevation)
    {
        if (gridPosition == position)
        {
            UpdateVerticalPosition();
        }
    }

    private void UpdateVerticalPosition()
    {
        transform.position = LevelGrid.Instance.GetWorldPosition(gridPosition);
    }

    public Team GetTeam()
    {
        return team;
    }

    public void Heal(int healAmount)
    {
        unitEnergySystem.Heal(healAmount);
    }

    public void Damage(int damageAmount)
    {
        unitEnergySystem.Damage(damageAmount);
    }

    public void AddFavor(int favorAmount)
    {
        favorSystem.Add(favorAmount);
    }

    // assumption for now is you can only use all favor
    public void UseFavor()
    {
        favorSystem.Remove(GetMaxFavor());

        OnAnyUnitExpendFavor?.Invoke(this);
    }

    private void EnergySystem_OnOutOfEnergy(object sender, EventArgs e)
    {
        if (DeathPrevention())
        {
            return;
        }
        actionPoints = 1;
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        inArena = false;
        gridPosition = ReserveGridSystem.Instance.AddUnit(GetTeam(), this);
        transform.position = ReserveGridSystem.Instance.GetWorldPosition(GetTeam(), gridPosition);

        OnAnyUnitOutOfEnergy?.Invoke(this);
    }

    public bool DeathPrevention()
    {
        foreach (Perk perk in basePerkArray)
        {
            if (perk.MeetsConditions(GameEvent.UnitEnergyDepleted, this, null))
            {
                perk.ApplyEffect(this); // right now only checks for first one
                return true;
            }
        }
        return false;
    }

    public int GetEnergy()
    {
        return unitEnergySystem.GetEnergy();
    }

    public int GetMaxEnergy()
    {
        return unitEnergySystem.GetMaxEnergy();
    }

    public float GetHealthNormalized()
    {
        return unitEnergySystem.GetEnergyNormalized();
    }

    public int GetMaxFavor()
    {
        return favorSystem.GetMaxFavor();
    }

    public float GetFavorNormalized()
    {
        return favorSystem.GetFavorNormalized();
    }

    public void SetInArena(bool inArena)
    {
        this.inArena = inArena;
    }

    public bool IsInArena()
    {
        return inArena;
    }

    public bool IsProne()
    {
        return isProne;
    }

    public Player GetPlayerData()
    {
        return playerData;
    }

    internal Stats GetStats()
    {
        return playerData.GetStats();
    }

    public int GetTotalArmorBonusFromPerks()
    {
        int totalBonus = 0;
        foreach (Perk perk in basePerkArray)
        {
            totalBonus += 0; //perk.GetArmorClassBonus();
        }
        return totalBonus;
    }

    public Perk[] GetPerks()
    {
        return basePerkArray;
    }

    public StatusEffect[] GetStatusEffects()
    {
        return baseStatusEffectArray;
    }


    internal void AddStatus(StatusEffect status)
    {
        throw new NotImplementedException();
    }

    internal void RemoveStatus(StatusEffect status)
    {
        throw new NotImplementedException();
    }
}