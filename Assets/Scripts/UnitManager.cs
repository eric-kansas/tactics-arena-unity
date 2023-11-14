using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public static UnitManager Instance { get; private set; }


    private List<Unit> unitList;
    private Dictionary<int, List<Unit>> teamUnitListDict;

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
        teamUnitListDict = new Dictionary<int, List<Unit>>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        //Unit.OnAnyUnitOutOfEnergy += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        unitList.Add(unit);

        int teamId = unit.GetTeam();
        if (!teamUnitListDict.ContainsKey(teamId))
        {
            teamUnitListDict[teamId] = new List<Unit>();
        }
        teamUnitListDict[teamId].Add(unit);
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        unitList.Remove(unit);
        int teamId = unit.GetTeam();
        if (teamUnitListDict.ContainsKey(teamId))
        {
            teamUnitListDict[teamId].Remove(unit);
        }
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public List<Unit> GetTeamUnitList(int teamId)
    {
        if (teamUnitListDict.TryGetValue(teamId, out var teamList))
        {
            return teamList;
        }
        return new List<Unit>(); // Return empty list if the team does not exist
    }

}
