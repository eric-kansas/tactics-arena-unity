using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Plain,
    Forest,
    Mountain,
    Water,
    Desert,
    Snow
}

public class GridObject
{

    private GridSystem<GridObject> gridSystem;
    private GridPosition gridPosition;
    private List<Unit> unitList;
    private IInteractable interactable;

    private TerrainType terrainType;
    private int elevation;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        unitList = new List<Unit>();
    }

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition, TerrainType terrainType, int elevation) : this(gridSystem, gridPosition)
    {
        this.terrainType = terrainType;
        SetElevation(elevation);
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit + "\n";
        }

        return gridPosition.ToString(); //+ "\n" + "Terrain: " + terrainType + ", Elevation: " + elevation + "\n" + unitString;
    }

    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public void ClearInteractable()
    {
        this.interactable = null;
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        this.terrainType = terrainType;
    }

    public TerrainType GetTerrainType()
    {
        return terrainType;
    }

    public void SetElevation(int elevation)
    {
        this.elevation = elevation;
    }

    public int GetElevation()
    {
        return elevation;
    }

}