using UnityEngine;

/// <summary>
/// Represents the unit's behavior when actively pursuing a target.
/// The unit moves towards its current target.
/// Transitions to CombatState if within attack range.
/// Transitions back to its primary stance if target is lost or dies.
/// </summary>
public class PursueState : UnitBaseState {
    private float _retargetingInterval = 1f;
    private float _nextTargetCheckTime = 0f;

    public PursueState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Pursue State, Target: {(_unit.Target != null ? _unit.Target.name : "null")}");
        if (_unit.Agent != null && _unit.Agent.isOnNavMesh) {
            _unit.Agent.isStopped = false;
            // Access AnimationController from the base Unit class
            if (_unit.AnimationController != null) {
                _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Moving_Forward); // Or a generic run/move
            }
        }
    }

    public override void UpdateState() {
        if (_unit.Target == null || _unit.Target.IsDead) {
            _unit.Target = null;
            // Access CurrentStance from the base Unit class
            if (_unit.CurrentStance == Unit.Stance.Offensive) {
                _stateMachine.ChangeState(new OffensiveState(_unit, _stateMachine));
            } else {
                _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
            }
            return;
        }

        // Access CurrentStance from the base Unit class
        if (_unit.CurrentStance == Unit.Stance.Defensive) {
            if (!_unit.IsTargetLocked && Vector2.Distance(_unit.transform.position, _unit.Target.transform.position) > _unit.AggroRadius) {
                _unit.Target = null;
                _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
                return;
            }
        }

        if (_nextTargetCheckTime <= Time.time) {
            _unit.FindAndSetClosestTarget(_unit.Faction == 0 ? 1 : 0);
            _nextTargetCheckTime = Time.time + _retargetingInterval;

            if (_unit.Agent != null && _unit.Agent.isOnNavMesh) {
                _unit.Agent.SetDestination(_unit.Target.transform.position);
            }

            if (Physics2D.Distance(_unit.Collider, _unit.Target.Collider).distance <= _unit.Range && !Physics2D.Linecast(_unit.transform.position, _unit.Target.transform.position, _unit.UnpierceableLayers)) {
                _stateMachine.ChangeState(new CombatState(_unit, _stateMachine));
                return;
            }
        }

        // Animation update based on agent's velocity for 2D sprite rotation (Y-axis only)
        if (_unit.AnimationController != null && _unit.Agent != null && _unit.Agent.velocity.sqrMagnitude > 0.01f) { // Check if moving
            Transform acTransform = _unit.AnimationController.transform;
            if (_unit.Agent.velocity.x > 0.01f) { // Moving right
                acTransform.rotation = Quaternion.Euler(0f, 0f, 0f); // Face right (0 degrees on Y)
            } else if (_unit.Agent.velocity.x < -0.01f) { // Moving left
                acTransform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face left (180 degrees on Y)
            }
        }
    }

    public override void ExitState() {
        // Debug.Log($"{_unit.gameObject.name} exiting Pursue State.");
    }
}
