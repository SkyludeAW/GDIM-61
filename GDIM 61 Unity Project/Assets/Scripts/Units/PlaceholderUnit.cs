using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlaceholderUnit : Unit {
    [SerializeField] protected float PlaceholderAttackRange;

    [SerializeField] private float _retargetingInterval = 1f;
    private float _nextTargetCheckTime = 0f;

    private void Awake() {
        Initialize();
    }

    private void Update() {
        // Faction 1 refers to enemy AI
        if (Faction == 1) {
            if (Time.time > _nextTargetCheckTime) {
                FindAndSetClosestTarget(0);
                _nextTargetCheckTime = Time.time + _retargetingInterval;
            }
        }

        // State machine for when there exists a target unit
        if (Target != null) {
            Agent.SetDestination(Target.transform.position);
            if (Physics2D.Distance(Collider, Target.Collider).distance <= PlaceholderAttackRange) {
                Agent.isStopped = true;
                if (Time.time >= NextAttackTime)
                    Attack(Target);
            } else {
                Agent.isStopped = false;
            }
        } else {
            if (!Agent.pathPending) {
                if (Agent.remainingDistance <= 0.01) {
                    Agent.isStopped = true;
                    Agent.ResetPath();
                } else {
                    Agent.isStopped = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            TakeDamage(25);
        }

    }

    private void Attack(Unit target) {
        target.TakeDamage(BaseDamage, (target.transform.position - transform.position) * KnockbackPower);
        NextAttackTime = Time.time + AttackCooldown;
    }

    protected override void Initialize() {
        base.Initialize();
        if (UnitsManager.Instance != null && !UnitsManager.Instance.GetUnitsInFaction(Faction).Contains(this)) {
            UnitsManager.Instance.RegisterUnit(this);
        }
        if (Faction == 0) {
            Controllable = true;
        } else {
            Controllable = false;
        }
    }

    // 似了
    public override void Die() {
        if (UnitsManager.Instance != null) {
            UnitsManager.Instance.UnregisterUnit(this);
        }
        Destroy(this.gameObject);
    }

    public override void PerformAttack(Unit targetUnit) {
        // throw new System.NotImplementedException();
    }
}
