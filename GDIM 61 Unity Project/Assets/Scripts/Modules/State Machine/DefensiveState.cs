using UnityEngine;

/// <summary>
/// Represents the unit's defensive stance.
/// In this state, the unit patrols around a center point and only engages targets within its aggro radius.
/// If no target is in aggro range, it transitions to WanderState.
/// If a target enters aggro range, it transitions to PursueState.
/// </summary>
public class DefensiveState : UnitBaseState {
    public DefensiveState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    private float _nextTargetCheckTime;
    private readonly float _retargetingInterval = 1f; // How often to scan for new targets

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Defensive State.");
    }

    public override void UpdateState() {
        if (Time.time >= _nextTargetCheckTime) {
            _unit.FindAndSetClosestTarget(_unit.Faction == 0 ? 1 : 0, Mathf.Max(_unit.AggroRadius, _unit.Range)); // Target opposite faction
            _nextTargetCheckTime = Time.time + _retargetingInterval;
        }

        // State transitions
        if (_unit.Target != null && !_unit.Target.IsDead) {
            _stateMachine.ChangeState(new PursueState(_unit, _stateMachine));
        } else {
            _unit.Target = null;
            _stateMachine.ChangeState(new IdleState(_unit, _stateMachine));
        }
    }

    public override void ExitState() {
        // Debug.Log($"{_unit.gameObject.name} exiting Defensive State.");
    }
}
