using System;

public interface IOpportunityAttack
{
    event Action OnAttackMissed;
    void TakeAction(GridPosition position, Action onComplete);
}