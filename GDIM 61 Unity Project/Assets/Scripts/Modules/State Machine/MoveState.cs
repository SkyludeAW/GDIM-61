using UnityEngine;
using UnityEngine.AI;

public class MoveState : UnitBaseState {
    Vector3 destination;
    NavMeshAgent agent;
    public MoveState(Unit unit, UnitStateMachine stateMachine, Vector3 destination) : base(unit, stateMachine) { 
        this.destination = destination;
        agent = unit.Agent;
    }

    public override void EnterState() {
        // Debug.Log($"{_unit.gameObject.name} entering Pursue State, Target: {(_unit.Target != null ? _unit.Target.name : "null")}");
        if (agent != null && agent.isOnNavMesh) {
            
            _unit.SetTargetDestination(destination);

            // Access AnimationController from the base Unit class
            if (_unit.AnimationController != null) {
                _unit.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Moving_Forward); // Or a generic run/move
            }
        }
    }

    public override void UpdateState() {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.01f && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)) {
            if (_unit.CurrentStance == Unit.Stance.Offensive) {
                _stateMachine.ChangeState(new OffensiveState(_unit, _stateMachine));
            } else { // Defensive stance
                _stateMachine.ChangeState(new DefensiveState(_unit, _stateMachine));
            }
        }

        // Animation update based on agent's velocity for 2D sprite rotation (Y-axis only)
        if (_unit.AnimationController != null && agent != null && agent.velocity.sqrMagnitude > 0.01f) { // Check if moving
            Transform acTransform = _unit.AnimationController.transform;
            if (agent.velocity.x > 0.01f) { // Moving right
                acTransform.rotation = Quaternion.Euler(0f, 0f, 0f); // Face right (0 degrees on Y)
            } else if (agent.velocity.x < -0.01f) { // Moving left
                acTransform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face left (180 degrees on Y)
            }
        }
    }
}
