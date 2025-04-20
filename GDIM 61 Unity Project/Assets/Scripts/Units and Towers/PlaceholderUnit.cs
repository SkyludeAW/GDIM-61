using UnityEngine;
using UnityEngine.AI;

public class PlaceholderUnit : Unit {
    [SerializeField] private int _faction;
    [SerializeField] protected float AttackRange;

    private void Awake() {
        Initialize();
    }

    private void Update() {
        // State machine for when there exists a target unit
        if (Target != null) {
            Agent.SetDestination(Target.transform.position);
            if (Physics2D.Distance(Collider, Target.Collider).distance <= AttackRange) {
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
        Controllable = true;
    }

    // 似了
    public override void Die() {
        Destroy(this.gameObject);
    }
}
