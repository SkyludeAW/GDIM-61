using UnityEngine;

/// <summary>
/// Represents the unit's behavior when in combat with a target.
/// The unit will stop moving and attack its target if the attack is off cooldown.
/// Transitions back to PursueState if the target moves out of attack range.
/// Transitions to its primary stance if the target dies or is lost.
/// </summary>
public class CombatState : UnitBaseState {
    private bool attackComplete;

    public CombatState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    public override void EnterState() {
        if (_unit.Agent != null && _unit.Agent.isOnNavMesh) {
            _unit.Agent.isStopped = true;
            _unit.Agent.ResetPath();
        }
        if (_unit.AnimationController != null) {
            _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Idle);
        }
        attackComplete = false;
    }

    public override void UpdateState() {
        if (_unit.Target == null || _unit.Target.IsDead) {
            _unit.Target = null;
            ReturnToDefaultState();
            return;
        }

        if (Physics2D.Distance(_unit.Collider, _unit.Target.Collider).distance > _unit.Range * 1.2f || Physics2D.Linecast(_unit.transform.position, _unit.Target.transform.position, _unit.UnpierceableLayers)) {
            _stateMachine.ChangeState(new PursueState(_unit, _stateMachine));
            return;
        }

        FaceTarget();

        if (!attackComplete && Time.time >= _unit.NextAttackTime) {
            _unit.PerformAttack(_unit.Target);
            attackComplete = true;
        }
    }

    private void ReturnToDefaultState() {
        if (_unit.CurrentStance == Unit.Stance.Offensive) {
            _stateMachine.ChangeState(new OffensiveState(_unit, _stateMachine));
        } else {
            _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
        }
    }

    /// <summary>
    /// Orients the unit's AnimationController to face the target using explicit Y-axis Euler angles.
    /// </summary>
    private void FaceTarget() {
        if (_unit.AnimationController == null || _unit.Target == null)
            return;

        Transform acTransform = _unit.AnimationController.transform;
        float xDifference = _unit.Target.transform.position.x - _unit.transform.position.x;

        if (xDifference > 0.01f) { // Target is to the right
            acTransform.rotation = Quaternion.Euler(0f, 0f, 0f);    // Face right
        } else if (xDifference < -0.01f) { // Target is to the left
            acTransform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face left
        }
    }

    public override void ExitState() {
    }
}
