using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{

    public static UnitActionSystem Instance { get; private set; }


    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy;
    private bool isPreviewingAction = false;
    private GridPosition actionPreviewGridPosition;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        selectedAction = null;
    }

    private void Start()
    {
        //SetSelectedUnit(selectedUnit);
    }

    private void Update()
    {
        if (isBusy)
        {
            return;
        }

        if (TurnSystem.Instance.GetCurrentTeam() != Match.Instance.GetClientTeam())
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (InputManager.Instance.IsRightMouseButtonDownThisFrame())
        {
            ClearActionPreview();
            return;
        }

        if (TryHandleUnitSelection())
        {
            return;
        }

        if (selectedAction == null)
        {
            return;
        }

        HandleSelectedAction();
    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsConfirmButtonDownThisFrame() && isPreviewingAction)
        {
            ConfirmAction();
            return;
        }

        if (InputManager.Instance.IsLeftMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (!selectedAction.IsActionApplicable())
            {
                // Activating from the wrong domain
                return;
            }

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                return;
            }

            if (!selectedAction.MeetsRequirements(mouseGridPosition))
            {
                return;
            }

            if (mouseGridPosition == actionPreviewGridPosition)
            {
                ConfirmAction();
            }

            PreviewAction(mouseGridPosition);

        }
    }

    private void PreviewAction(GridPosition gridPosition)
    {
        selectedAction.ClearPreview();

        actionPreviewGridPosition = gridPosition;
        isPreviewingAction = true;

        selectedAction.PreviewAction(gridPosition);
    }

    private void ConfirmAction()
    {
        if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
        {
            return;
        }

        SetBusy();
        selectedAction.TakeAction(actionPreviewGridPosition, ClearBusy);
        OnActionStarted?.Invoke(this, EventArgs.Empty);
        isPreviewingAction = false;
    }

    private void SetBusy()
    {
        isBusy = true;

        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;

        OnBusyChanged?.Invoke(this, isBusy);
    }

    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsLeftMouseButtonDownThisFrame())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        // Unit is already selected
                        return false;
                    }

                    if (unit.GetTeam() != Match.Instance.GetClientTeam())
                    {
                        // Clicked on an Enemy
                        return false;
                    }

                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }

        return false;
    }

    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        selectedAction = null;
        //SetSelectedAction(unit.GetAction<MoveAction>());

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        if (selectedAction != null && selectedAction != baseAction)
        {
            ClearActionPreview();
        }
        selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearActionPreview()
    {
        if (isPreviewingAction)
        {
            // Clear the preview visuals
            // Optionally call a method on the current action to clear its preview
            selectedAction.ClearPreview();
            isPreviewingAction = false;
        }
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

}