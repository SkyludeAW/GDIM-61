using UnityEngine;

/// <summary>
/// Represents the unit's offensive stance.
/// In this state, the unit actively seeks the nearest enemy target globally.
/// If no target is found, it transitions to IdleState.
/// If a target is found, it transitions to PursueState.
/// </summary>
public class OffensiveState : UnitBaseState {
    private float _nextTargetCheckTime;
    private readonly float _retargetingInterval = 1f; // How often to scan for new targets

    public OffensiveState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Offensive State.");
        _nextTargetCheckTime = Time.time; // Check for target immediately upon entering
    }

    public override void UpdateState() {
        // Periodically scan for the closest target
        if (Time.time >= _nextTargetCheckTime) {
            // In offensive mode, search without a radius limit (global search)
            _unit.FindAndSetClosestTarget(_unit.Faction == 0 ? 1 : 0); // Target opposite faction
            _nextTargetCheckTime = Time.time + _retargetingInterval;
        }

        // State transitions based on target presence
        if (_unit.Target != null && !_unit.Target.IsDead) {
            _stateMachine.ChangeState(new PursueState(_unit, _stateMachine));
        } else {
            // If no target is found or current target is dead, go to Idle
            _stateMachine.ChangeState(new IdleState(_unit, _stateMachine));
        }
    }

    public override void ExitState() {
        // Debug.Log($"{_unit.gameObject.name} exiting Offensive State.");
    }
}
