using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents the unit's wandering behavior when in a Defensive stance and no target is in aggro range.
/// The unit will move to random points within its aggro radius around its patrol center.
/// It will transition back to DefensiveState to re-evaluate targets.
/// </summary>
public class WanderState : UnitBaseState {
    private readonly float _wanderRadiusMultiplier = 0.8f;
    private float _timeToReEvaluate = 2.0f;
    private float _timeEntered;

    public WanderState(Unit unit, UnitStateMachine stateMachine) : base(unit, stateMachine) { }

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Wander State.");
        _timeEntered = Time.time;
        if (_unit.Agent != null && _unit.Agent.isOnNavMesh) {
            _unit.Agent.isStopped = false;
            SetNewWanderDestination();
        }
        // Access AnimationController from the base Unit class
        if (_unit.AnimationController != null) {
            _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Moving_Forward);
        }
    }

    public override void UpdateState() {
        if (_unit.Agent == null || !_unit.Agent.isOnNavMesh) {
            if (Time.time >= _timeEntered + _timeToReEvaluate) {
                _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
            }
            return;
        }

        if (!_unit.Agent.pathPending && _unit.Agent.remainingDistance <= _unit.Agent.stoppingDistance) {
            SetNewWanderDestination();
        }

        if (Time.time >= _timeEntered + _timeToReEvaluate) {
            _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
        }

        // Animation update based on agent's velocity for 2D sprite rotation
        if (_unit.AnimationController != null && _unit.Agent != null && _unit.Agent.velocity.sqrMagnitude > 0.01f) { // Check if moving
            Transform acTransform = _unit.AnimationController.transform;
            if (_unit.Agent.velocity.x > 0.01f) { // Moving right
                acTransform.rotation = Quaternion.LookRotation(Vector3.forward);
            } else if (_unit.Agent.velocity.x < -0.01f) { // Moving left
                acTransform.rotation = Quaternion.LookRotation(Vector3.back);
            }
        } else if (_unit.AnimationController != null && _unit.Agent != null && _unit.Agent.velocity.sqrMagnitude <= 0.01f) {
            // If stopped, might revert to an Idle animation, or maintain last facing direction's idle
            _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Idle);
        }
    }

    private void SetNewWanderDestination() {
        if (_unit.Agent == null || !_unit.Agent.isOnNavMesh)
            return;
        Vector2 randomDirection = Random.insideUnitCircle * (_unit.AggroRadius * _wanderRadiusMultiplier);
        Vector3 newDestination = _unit.PatrolCenter + new Vector3(randomDirection.x, randomDirection.y, _unit.transform.position.z);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDestination, out hit, _unit.AggroRadius, NavMesh.AllAreas)) {
            _unit.Agent.SetDestination(hit.position);
        } else {
            _unit.Agent.SetDestination(_unit.PatrolCenter);
        }
        _timeEntered = Time.time;
    }

    public override void ExitState() {
        // Debug.Log($"{_unit.gameObject.name} exiting Wander State.");
    }
}
