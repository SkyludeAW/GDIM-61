using UnityEngine;

/// <summary>
/// Represents the unit's idle behavior when in an Offensive stance and no target is available.
/// The unit will remain stationary.
/// It will transition back to its primary stance (Offensive/Defensive) to re-evaluate targets.
/// </summary>
public class IdleState : UnitBaseState {
    private float _timeToReEvaluate = 1.0f; // After how long to check for targets again
    private float _timeEntered;

    // Constructor now takes 'Unit' instead of 'BasicUnit'
    public IdleState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Idle State.");
        if (_unit.Agent != null && _unit.Agent.isOnNavMesh) {
            _unit.Agent.isStopped = true;
            if (!_unit.Agent.pathPending) {
                _unit.Agent.ResetPath();
            }
        }
        // Access AnimationController from the base Unit class
        if (_unit.AnimationController != null) {
            _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Idle);
        }
        _timeEntered = Time.time;
    }

    public override void UpdateState() {
        if (Time.time >= _timeEntered + _timeToReEvaluate) {
            // Access CurrentStance from the base Unit class
            if (_unit.CurrentStance == Unit.Stance.Offensive) {
                _stateMachine.ChangeState(new OffensiveState(_unit, _stateMachine));
            } else { // Defensive stance
                _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
            }
        }
    }

    public override void ExitState() {
        // Debug.Log($"{_unit.gameObject.name} exiting Idle State.");
        if (_unit.Agent != null && _unit.Agent.isOnNavMesh && _unit.Agent.isStopped) {
            _unit.Agent.isStopped = false;
        }
    }
}
