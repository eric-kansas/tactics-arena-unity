using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 3;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static Action<Unit> OnAnyUnitOutOfEnergy;
    public static Action<Unit> OnAnyUnitExpendFavor;

    [SerializeField] private Team team;

    private GridPosition gridPosition;
    private HealthSystem healthSystem;
    private UnitFavor favorSystem;
    private BaseAction[] baseActionArray;
    private int actionPoints = ACTION_POINTS_MAX;
    private bool inArena = true;

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

        healthSystem.OnDead += HealthSystem_OnDead;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void Setup(Team team, Player player)
    {
        healthSystem = GetComponent<HealthSystem>();
        favorSystem = GetComponent<UnitFavor>();
        baseActionArray = new BaseAction[0];

        this.team = team;
        this.name = player.name;
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

        // Re-fetch the baseActionArray to include the newly added abilities
        baseActionArray = GetComponents<BaseAction>();
    }

    private void Update()
    {
        if (inArena)
        {
            GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
            if (newGridPosition != gridPosition)
            {
                // Unit changed Grid Position
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
        healthSystem.Heal(healAmount);
    }

    public void Damage(int damageAmount)
    {
        healthSystem.Damage(damageAmount);
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

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        actionPoints = 0;
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        inArena = false;
        gridPosition = ReserveGridSystem.Instance.AddUnit(GetTeam(), this);
        transform.position = ReserveGridSystem.Instance.GetWorldPosition(GetTeam(), gridPosition);
        //Destroy(gameObject);

        OnAnyUnitOutOfEnergy?.Invoke(this);
    }

    public int GetMaxHealth()
    {
        return healthSystem.GetMaxHealth();
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
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

}