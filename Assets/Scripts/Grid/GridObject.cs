using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    // Natural Landscapes
    Plain,        // Grasslands, open fields
    Meadow,       // Grasslands, often with flowers
    Savanna,      // Grasslands with sparse trees
    Forest,       // Dense woodlands, thick foliage
    Taiga,        // Coniferous forests, subarctic
    Mountain,     // High altitude, rocky terrain
    Hills,        // Gentle slopes, less steep than mountains
    Badlands,     // Eroded rock formations
    Desert,       // Arid, sandy landscapes
    Tundra,       // Cold, treeless plains
    Glacier,      // Icy, frozen landscapes
    Snow,         // Cold, snow-covered areas

    // Water Bodies and Wetlands
    Water,        // Lakes, rivers, oceans
    Swamp,        // Wetlands, marshy areas
    Marsh,        // Wetlands, often waterlogged
    Sand,        // Sandy shores, coastal areas
    CoralReef,    // Marine ecosystems, underwater
    Oasis,        // Fertile areas in deserts

    // Special Terrains
    LavaField     // Molten lava areas
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

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

}