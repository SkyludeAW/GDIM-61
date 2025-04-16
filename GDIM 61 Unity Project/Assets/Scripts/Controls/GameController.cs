using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    private Vector2 _clickStartPosition;
    [SerializeField] private LayerMask _unitLayer;
    [SerializeField] private Transform _selectionAreaTransform;

    private List<Unit> _selectedUnits;

    private void Awake() {
        _selectedUnits = new List<Unit>();
        _selectionAreaTransform.gameObject.SetActive(false);
    }

    private void Update() {
        // Left mouse button down
        if (Input.GetMouseButtonDown(0)) {
            _clickStartPosition = GetMouseWorldPosition();
            _selectionAreaTransform.gameObject.SetActive(true);
        }

        // Right mouse button down
        if (Input.GetMouseButtonDown(1)) {
            Vector3 currentMousePosition = GetMouseWorldPosition();
            Collider2D selectedCollider = Physics2D.OverlapPoint(currentMousePosition);
            if (selectedCollider != null) {
                Unit selectedTarget = selectedCollider.GetComponent<Unit>();
                if (selectedTarget != null) {
                    foreach (Unit unit in _selectedUnits) {
                        if (unit != selectedTarget)
                            unit.SetTarget(selectedTarget);
                    }
                }
            } else {
                foreach (Unit unit in _selectedUnits) {
                    unit.SetTargetDestination(currentMousePosition);
                }
            }
        }

        // Left mouse button held
        if (Input.GetMouseButton(0)) {
            Vector3 currentMousePosition = GetMouseWorldPosition();
            Vector3 selectionLowerLeft = new Vector3(Mathf.Min(_clickStartPosition.x, currentMousePosition.x), Mathf.Min(_clickStartPosition.y, currentMousePosition.y));
            Vector3 selectionUpperRight = new Vector3(Mathf.Max(_clickStartPosition.x, currentMousePosition.x), Mathf.Max(_clickStartPosition.y, currentMousePosition.y));
            _selectionAreaTransform.position = selectionLowerLeft;
            _selectionAreaTransform.localScale = selectionUpperRight - selectionLowerLeft;
        }

        // Left mouse button up 
        if (Input.GetMouseButtonUp(0)) {
            _selectionAreaTransform.gameObject.SetActive(false);
            foreach (Unit unit in _selectedUnits) {
                unit.SetSelectionActive(false);
            }
            _selectedUnits.Clear();

            Collider2D[] selectedUnits = Physics2D.OverlapAreaAll(_clickStartPosition, GetMouseWorldPosition());

            foreach (Collider2D collider in selectedUnits) {
                Unit unit = collider.GetComponent<Unit>();
                if (unit != null && unit.Selectable) {
                    _selectedUnits.Add(unit);
                    unit.SetSelectionActive(true);
                }
            }
        }
    }

    public static Vector2 GetMouseWorldPosition() {
        return CameraLocator.Instance.PlayerCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}
