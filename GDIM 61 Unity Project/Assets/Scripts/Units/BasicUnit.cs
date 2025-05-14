using System.Linq;
using UnityEngine;

public class BasicUnit : Unit {
    [SerializeField] private float _retargetingInterval = 1f;
    private float _nextTargetCheckTime = 0f;

    private BehaviorState _currentState;

    [SerializeField] private AnimationController _animationController;

    [SerializeField] private LayerMask _unpierceableLayers;
    [SerializeField] private Attack _attack;

    enum BehaviorState {
        Idle, Moving, AttackPrep, Attacking
    }

    private void Awake() {
        Initialize();
    }

    private void Update() {

        switch (Faction) {
            case 0:
                if (!Controllable && _currentState != BehaviorState.Attacking) {
                    if (Time.time > _nextTargetCheckTime) {
                        FindAndSetClosestTarget(1);
                        _nextTargetCheckTime = Time.time + _retargetingInterval;
                    }
                }

                break;

            // Faction 1 refers to enemy AI
            case 1:
                if (Time.time > _nextTargetCheckTime) {
                    FindAndSetClosestTarget(0);
                    _nextTargetCheckTime = Time.time + _retargetingInterval;
                }
                break;
        }

        // Sets the current behavior state
        if (_currentState != BehaviorState.Attacking)
            if (Target != null) {
                Agent.SetDestination(Target.transform.position);
                if (Physics2D.Distance(Collider, Target.Collider).distance <= AttackRange && !Physics2D.Linecast(transform.position, Target.transform.position, _unpierceableLayers)) {
                    _currentState = BehaviorState.AttackPrep;
                } else {
                    _currentState = BehaviorState.Moving;
                }
            } else {
                if (!Agent.pathPending)
                    if (Agent.remainingDistance > Agent.stoppingDistance) {
                        _currentState = BehaviorState.Moving;
                    } else {
                        _currentState = BehaviorState.Idle;
                    }
            }

        // Acts upon the current behavior state
        switch (_currentState) {
            case BehaviorState.Moving:
                Agent.isStopped = false;

                #region Animation
                if (Agent.velocity.y > 0) {
                    _animationController.ChangeAnimationState(AnimationController.AnimationState.Moving_Backward);
                } else if (Agent.velocity.y < 0) {
                    _animationController.ChangeAnimationState(AnimationController.AnimationState.Moving_Forward);
                }
                if (Agent.velocity.x > 0) {
                    _animationController.SR.flipX = false;
                } else if (Agent.velocity.x < 0) {
                    _animationController.SR.flipX = true;
                }
                #endregion

                break;

            case BehaviorState.Idle:
                Agent.isStopped = true;
                if (!Agent.pathPending)
                    Agent.ResetPath();

                #region Animation
                _animationController.ChangeAnimationState(AnimationController.AnimationState.Idle);
                if (Agent.velocity.x > 0) {
                    _animationController.SR.flipX = false;
                } else if (Agent.velocity.x < 0) {
                    _animationController.SR.flipX = true;
                }
                #endregion

                break;

            case BehaviorState.AttackPrep:
                Agent.isStopped = true;
                Agent.ResetPath();
                if (Target == null)
                    return;
                float xDifferenceWithTarget = Target.transform.position.x - transform.position.x;
                _animationController.ChangeAnimationState(AnimationController.AnimationState.Idle);
                if (xDifferenceWithTarget > 0) {
                    _animationController.SR.flipX = false;
                } else if (xDifferenceWithTarget < 0) {
                    _animationController.SR.flipX = true;
                }
                if (Time.time >= NextAttackTime) {
                    Attack(Target);
                    _currentState = BehaviorState.Attacking;
                }

                break;

            case BehaviorState.Attacking:
                Agent.isStopped = true;
                break;

        }
    }

    private void Attack(Unit target) {
        if (_attack != null) {
            _attack.Initialize(BaseDamage, target, ((target.transform.position - transform.position)).normalized * KnockbackPower, this);
        }
        _attack.Execute();

        // target.TakeDamage(BaseDamage, (target.transform.position - transform.position) * KnockbackPower);
    }

    public void AttackComplete() {
        _currentState = BehaviorState.AttackPrep;
        NextAttackTime = Time.time + AttackCooldown;
    }

    protected override void Initialize() {
        base.Initialize();
        if (UnitsManager.Instance != null && !UnitsManager.Instance.GetUnitsInFaction(Faction).Contains(this)) {
            UnitsManager.Instance.RegisterUnit(this);
        }
    }

    public override void Die() {
        if (UnitsManager.Instance != null) {
            UnitsManager.Instance.UnregisterUnit(this);
        }
        Destroy(this.gameObject);
    }
}