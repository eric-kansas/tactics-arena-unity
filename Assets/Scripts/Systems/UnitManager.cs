using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public static UnitManager Instance { get; private set; }


    private List<Unit> arenaUnitList;
    private Dictionary<Team, List<Unit>> teamUnitListDict;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        arenaUnitList = new List<Unit>();
        teamUnitListDict = new Dictionary<Team, List<Unit>>();
    }

    private void Start()
    {
        Unit.OnAnyUnitInitialized += Unit_OnAnyUnitInitialized;
        Unit.OnAnyUnitOutOfEnergy += Unit_OnAnyUnitOutOfEnergy;
    }

    private void Unit_OnAnyUnitInitialized(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        arenaUnitList.Add(unit);

        Team team = unit.GetTeam();
        if (!teamUnitListDict.ContainsKey(team))
        {
            teamUnitListDict[team] = new List<Unit>();
        }
        teamUnitListDict[team].Add(unit);
    }

    private void Unit_OnAnyUnitOutOfEnergy(Unit unit)
    {
        arenaUnitList.Remove(unit);
    }

    public List<Unit> GetArenaUnitList()
    {
        return arenaUnitList;
    }

    public List<Unit> GetTeamUnitList(Team teamId)
    {
        if (teamUnitListDict.TryGetValue(teamId, out var teamList))
        {
            return teamList;
        }
        return new List<Unit>(); // Return empty list if the team does not exist
    }

    public List<Unit> GetTeamArenaUnitList(Team teamId)
    {
        List<Unit> arenaUnits = new List<Unit>();
        foreach (Unit unit in GetTeamUnitList(teamId))
        {
            if (unit.IsInArena())
            {
                arenaUnits.Add(unit);
            }
        }
        return arenaUnits;
    }

}
