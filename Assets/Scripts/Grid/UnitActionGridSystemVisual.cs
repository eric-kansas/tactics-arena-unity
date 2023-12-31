using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GridVisualTypeMaterial
{
    public GridVisualType gridVisualType;
    public Material material;
}

public enum GridVisualType
{
    White,
    Blue,
    Red,
    RedSoft,
    Yellow,
    Green,
    Purple,


}

public class UnitActionGridSystemVisual : MonoBehaviour
{

    public static UnitActionGridSystemVisual Instance { get; private set; }
    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;


    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;
    private Zone actionZone;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionGridSystemVisual! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;

        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

        LevelGrid.Instance.OnElevationChanged += LevelGrid_OnElevationChange;
        FogOfWarSystem.OnTeamVisibilityChanged += FogOfWarSystem_OnTeamVisibilityChanged;
    }

    private void UnitActionSystem_OnSelectedUnitChanged()
    {
        UpdateGridVisual();
    }

    private void UnitActionSystem_OnSelectedActionChanged()
    {
        UpdateGridVisual();
    }

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        HideAllGridPosition();
        HideBorder();
    }

    private void FogOfWarSystem_OnTeamVisibilityChanged(Team team)
    {
        UpdateGrid();
    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void LevelGrid_OnElevationChange(GridPosition position, int arg2)
    {
        gridSystemVisualSingleArray[position.x, position.z].transform.position = FogOfWarSystem.Instance.GetPerceivedWorldPosition(position);
    }

    private void Start()
    {
        BuildGrid();
        UpdateGridVisual();
    }

    private void BuildGrid()
    {
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;
        gridSystemVisualSingleArray = new GridSystemVisualSingle[diameter, diameter];

        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z); // Adjust for circle center
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    Vector3 worldPos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(gridPosition);
                    worldPos.y += 0.01f;
                    Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, worldPos, Quaternion.identity);

                    gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
        }
    }

    private void UpdateGrid()
    {
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    Vector3 worldPos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(gridPosition);
                    worldPos.y += 0.01f;
                    gridSystemVisualSingleArray[x, z].transform.position = worldPos;
                }
            }
        }
    }

    public void HideAllGridPosition()
    {
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    gridSystemVisualSingleArray[x, z].Hide();
                }
            }
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > range)
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    private void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            if (gridSystemVisualSingleArray[gridPosition.x, gridPosition.z] == null)
            {
                Debug.Log("position null: " + gridPosition);
            }
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].
                Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private void UpdateGridVisual()
    {
        HideAllGridPosition();
        HideBorder();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        if (selectedAction == null)
        {
            return;
        }

        GridVisualType gridVisualType;

        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case FavorAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case RangeAction shootAction:
                gridVisualType = GridVisualType.Red;
                if (selectedUnit.IsInArena())
                {
                    ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft);
                }
                break;
            case GrenadeAction grenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case MeleeAction swordAction:
                gridVisualType = GridVisualType.Red;
                if (selectedUnit.IsInArena())
                {
                    ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);
                }
                break;
            case InteractAction interactAction:
                gridVisualType = GridVisualType.Blue;
                break;
        }

        List<GridPosition> validActionGridPositionList = selectedAction.GetValidActionGridPositionList();
        ShowGridPositionList(validActionGridPositionList, gridVisualType);

        // add zone highlight
        validActionGridPositionList.Add(selectedUnit.GetGridPosition());
        actionZone = new Zone(validActionGridPositionList);
        actionZone.ShowBorder(Color.blue);
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }

        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }

    private void HideBorder()
    {
        if (actionZone != null)
        {
            actionZone.HideBorder();
        }
    }

}