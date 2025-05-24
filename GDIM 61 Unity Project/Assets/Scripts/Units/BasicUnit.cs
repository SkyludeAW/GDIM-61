using UnityEngine;

/// <summary>
/// Represents a basic unit in the game, utilizing a state machine for behavior.
/// Inherits from the abstract Unit class and implements specific behaviors like PerformAttack.
/// </summary>
public class BasicUnit : Unit {

    [Header("BasicUnit Specifics")]
    [SerializeField] private Attack _attack; // The attack behavior component specific to BasicUnit
    [SerializeField] private bool _overrideStats;
    [SerializeField] protected float maxHitPoint;
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected float knockbackPower;
    [SerializeField] protected float knockbackResistance = 0f;

    [Tooltip("Aggro radius specific override for this BasicUnit. If 0, uses Unit.AggroRadius (from Card or default).")]
    [SerializeField] private float _overrideAggroRadius = 0f;

    [SerializeField] private bool _overrideStance;
    [SerializeField] private Unit.Stance _overriddenStance;

    private UnitStateMachine _stateMachine; // The state machine instance governing this unit's behavior

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the unit's stats and then configures its faction and state machine.
    /// </summary>
    private void Awake() {
        base.Initialize(); // Sets up stats from Card Base. Does NOT set faction or stance yet.

        // Configure faction using the value set in the Inspector (prefab's default faction).
        // This will set Unit.Faction, Unit.CurrentStance, and then call OnFactionConfigured().
        ConfigureFaction(this.Faction); // this.Faction here is the Inspector value before ConfigureFaction runs.
        if (_overrideStance)
            SetStance(_overriddenStance);

        if (_overrideStats) {
            MaxHitPoint = maxHitPoint;
            HitPoint = MaxHitPoint;
            Speed = speed;
            BaseDamage = baseDamage;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            KnockbackPower = knockbackPower;
            KnockbackResistance = knockbackResistance;

            if (Agent != null && Agent.enabled)
                Agent.speed = Speed;
        }
    }

    /// <summary>
    /// Called by Unit.ConfigureFaction after Faction and CurrentStance (derived from Faction) are set.
    /// Responsible for initializing or updating the state machine.
    /// </summary>
    protected override void OnFactionConfigured() {
        // Apply aggro radius override if specified for this BasicUnit type
        if (_overrideAggroRadius > 0.01f) {
            AggroRadius = _overrideAggroRadius;
        }
        // Else, it uses the AggroRadius set in Unit.Initialize (from Card or default)

        // Debug.Log($"{gameObject.name} OnFactionConfigured. Faction: {Faction}, Stance: {CurrentStance}. Initializing/Updating SM.");
        UpdateStateMachineForCurrentStance();
    }

    /// <summary>
    /// Called by Unit.ManualSetStance after CurrentStance is explicitly changed.
    /// Responsible for updating the state machine.
    /// </summary>
    protected override void OnStanceChanged() {
        // Debug.Log($"{gameObject.name} OnStanceManuallyChanged. New Stance: {CurrentStance}. Updating SM.");
        UpdateStateMachineForCurrentStance();
    }

    /// <summary>
    /// Initializes or changes the state of the state machine based on the unit's CurrentStance.
    /// </summary>
    private void UpdateStateMachineForCurrentStance() {
        bool isNewStateMachine = _stateMachine == null;
        if (isNewStateMachine) {
            _stateMachine = new UnitStateMachine();
        }

        // CurrentStance is already correctly set in the base Unit class
        if (CurrentStance == Stance.Offensive) {
            if (isNewStateMachine)
                _stateMachine.Initialize(new OffensiveState(this, _stateMachine));
            else
                _stateMachine.ChangeState(new OffensiveState(this, _stateMachine));
        } else { // Defensive (or any other future stances that default to defensive-like behavior)
            if (isNewStateMachine)
                _stateMachine.Initialize(new DefensiveState(this, _stateMachine));
            else
                _stateMachine.ChangeState(new DefensiveState(this, _stateMachine));
        }
    }


    /// <summary>
    /// Called every frame. Updates the current state of the state machine.
    /// </summary>
    private void Update() {
        if (IsDead)
            return;
        _stateMachine?.Update();
    }

    /// <summary>
    /// Implements the abstract PerformAttack method from the Unit class.
    /// </summary>
    public override void PerformAttack(Unit targetUnit) {
        if (_attack == null || targetUnit == null || targetUnit.IsDead) {
            if (CurrentStance == Unit.Stance.Offensive) {
                _stateMachine.ChangeState(new OffensiveState(this, _stateMachine));
            } else {
                _stateMachine.ChangeState(new DefensiveState(this, _stateMachine));
            }
            return;
        }

        

        Vector2 knockbackDirection = (targetUnit.transform.position - transform.position).normalized;
        _attack.Initialize(BaseDamage, targetUnit, knockbackDirection * KnockbackPower, this);
        _attack.Execute();
    }

    public void AttackComplete() {
        _stateMachine.ChangeState(new PursueState(this, _stateMachine));
    }

    public void AttackTriggered() {
        NextAttackTime = Time.time + AttackCooldown;
    }

    public override void Die() {
        IsDead = true;
        if (Agent != null && Agent.isOnNavMesh) {
            Agent.isStopped = true;
            Agent.enabled = false;
        }
        // Unregistration from UnitsManager is handled by Unit.OnDisable
        if (Collider != null) {
            Collider.enabled = false;
        }
        Destroy(this.gameObject, 0.1f);
    }
}
