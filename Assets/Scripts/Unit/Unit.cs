using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 5;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitInitialized;
    public static Action<Unit> OnAnyUnitOutOfEnergy;
    public static Action<Unit> OnAnyUnitExpendFavor;

    [SerializeField] private Team team;
    private Player playerData;
    private GridPosition gridPosition;
    private UnitEnergySystem unitEnergySystem;
    private UnitFavor favorSystem;
    private BaseAction[] baseActionArray;
    private int actionPoints = ACTION_POINTS_MAX;
    [SerializeField] private bool inArena = true;
    [SerializeField] private SkinnedMeshRenderer renderer;

    private bool isProne;

    private void Awake()
    {

    }

    private void Start()
    {
        //gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        //LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);
        //transform.position = LevelGrid.Instance.GetWorldPosition(gridPosition);

        LevelGrid.Instance.OnElevationChanged += LevelGrid_OnElevationChange;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        unitEnergySystem.OnOutOfEnergy += EnergySystem_OnOutOfEnergy;

        OnAnyUnitInitialized?.Invoke(this, EventArgs.Empty);
    }

    public void Setup(Team team, Player player)
    {
        unitEnergySystem = GetComponent<UnitEnergySystem>();
        favorSystem = GetComponent<UnitFavor>();
        baseActionArray = new BaseAction[0];

        this.team = team;
        playerData = player;

        unitEnergySystem.SetMaxHealth(CalculateMaxEnergy());

        MoveAction moveAction = GetComponent<MoveAction>();
        moveAction.SetMaxMove(playerData.GetStats().MoveValue);

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

        renderer.material = team.GetMaterial();
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

    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T)
            {
                return (T)baseAction;
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

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;

        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.GetCurrentTeam() == GetTeam() && inArena)
        {
            actionPoints = ACTION_POINTS_MAX;

            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
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
        actionPoints = 1;
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        inArena = false;
        gridPosition = ReserveGridSystem.Instance.AddUnit(GetTeam(), this);
        transform.position = ReserveGridSystem.Instance.GetWorldPosition(GetTeam(), gridPosition);

        OnAnyUnitOutOfEnergy?.Invoke(this);
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

    private int CalculateMaxEnergy()
    {
        // Example calculation: base HP + (Constitution * factor)
        int baseHP = 10;
        int factor = 10;
        return baseHP + (playerData.GetStats().Endurance * factor);
    }

    public int CalculateArmorClass()
    {
        // Example calculation based on Dexterity and equipment
        int baseArmorClass = 10;
        int dexterityBonus = playerData.GetStats().Agility / 2;
        int equipmentArmorBonus = playerData.GetGear().GetTotalArmorBonus();
        return baseArmorClass + dexterityBonus + equipmentArmorBonus;
    }
}