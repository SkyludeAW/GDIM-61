using UnityEngine;
using UnityEngine.AI;

public class Placeholder : Unit {
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
        } else if (Agent.remainingDistance <= Agent.stoppingDistance && !Agent.hasPath) {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            TakeDamage(25);
        }
    }

    private void Attack(Unit target) {
        target.TakeDamage(BaseDamage);
        NextAttackTime = Time.time + AttackCooldown;
    }

    protected override void Initialize() {
        base.Initialize();
        SetFaction(_faction);
        Controllable = true;
    }

    // 似了
    public override void Die() {
        Destroy(this.gameObject);
    }
}
