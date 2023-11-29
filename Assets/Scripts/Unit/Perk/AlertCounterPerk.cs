public class AlertCounterPerk : Perk
{
    private bool hasCounterAvailable = true;

    public override void ApplyEffect(Unit unit, Unit attacker)
    {
        if (hasCounterAvailable && !FogOfWarSystem.Instance.IsVisible(unit.GetTeam(), attacker.GetGridPosition()))
        {
            // Counter-attack logic
            //unit.CounterAttack(attacker);
            hasCounterAvailable = false; // Use up the counter for this turn/round
        }
    }

    public override void ResetPerk()
    {
        hasCounterAvailable = true; // Reset the perk at the start of each turn/round
    }
}
