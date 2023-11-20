using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public static UnitManager Instance { get; private set; }


    private List<Unit> unitList;
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

        unitList = new List<Unit>();
        teamUnitListDict = new Dictionary<Team, List<Unit>>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitOutOfEnergy += Unit_OnAnyUnitOutOfEnergy;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        unitList.Add(unit);

        Team team = unit.GetTeam();
        if (!teamUnitListDict.ContainsKey(team))
        {
            teamUnitListDict[team] = new List<Unit>();
        }
        teamUnitListDict[team].Add(unit);
    }

    private void Unit_OnAnyUnitOutOfEnergy(Unit unit)
    {
        unitList.Remove(unit);
        Team teamId = unit.GetTeam();
        if (teamUnitListDict.ContainsKey(teamId))
        {
            teamUnitListDict[teamId].Remove(unit);
        }
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public List<Unit> GetTeamUnitList(Team teamId)
    {
        if (teamUnitListDict.TryGetValue(teamId, out var teamList))
        {
            return teamList;
        }
        return new List<Unit>(); // Return empty list if the team does not exist
    }

}
