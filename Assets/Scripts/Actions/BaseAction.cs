using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionUsage
{
    Arena,
    Reserves,
    Both
}


public abstract class BaseAction : MonoBehaviour
{

    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;


    protected Unit unit;
    protected bool isActive;
    protected ActionUsage actionUsage = ActionUsage.Arena;
    protected Action onActionComplete;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();

    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }

    public virtual bool MeetsRequirements(GridPosition gridPosition)
    {
        return true;
    }

    public abstract List<GridPosition> GetValidActionGridPositionList();

    public virtual int GetActionPointsCost()
    {
        return 1;
    }
    public virtual void PreviewAction(GridPosition gridPosition)
    {
    }

    public virtual void ClearPreview()
    {
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            // No possible Enemy AI Actions
            return null;
        }

    }

    public bool IsActionApplicable()
    {
        if (actionUsage == ActionUsage.Both) return true;

        bool isInArena = unit.IsInArena();
        return (actionUsage == ActionUsage.Arena && isInArena) ||
               (actionUsage == ActionUsage.Reserves && !isInArena);
    }

    public ActionUsage GetActionUsage()
    {
        return actionUsage;
    }

    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);

}