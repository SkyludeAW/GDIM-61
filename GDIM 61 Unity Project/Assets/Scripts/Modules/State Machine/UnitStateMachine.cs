using UnityEngine;

/// <summary>
/// Manages the states of a unit.
/// </summary>
public class UnitStateMachine {
    /// <summary>
    /// The current active state of the unit.
    /// </summary>
    public UnitBaseState CurrentState { get; private set; }

    /// <summary>
    /// Initializes the state machine with a starting state.
    /// </summary>
    /// <param name="startingState">The initial state for the unit.</param>
    public void Initialize(UnitBaseState startingState) {
        CurrentState = startingState;
        if (CurrentState != null) {
            CurrentState.EnterState();
        } else {
            Debug.LogError("UnitStateMachine initialized with a null starting state.");
        }
    }

    /// <summary>
    /// Transitions the unit to a new state.
    /// Calls ExitState on the current state and EnterState on the new state.
    /// </summary>
    /// <param name="newState">The state to transition to.</param>
    public void ChangeState(UnitBaseState newState) {
        if (CurrentState != null) {
            CurrentState.ExitState();
        }

        CurrentState = newState;

        if (CurrentState != null) {
            CurrentState.EnterState();
        } else {
            Debug.LogError("UnitStateMachine changed to a null state.");
        }
    }

    /// <summary>
    /// Updates the current state. This should be called every frame.
    /// </summary>
    public void Update() {
        if (CurrentState != null) {
            CurrentState.UpdateState();
        }
    }
}
