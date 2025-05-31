using UnityEngine;

public class Orion : Unit {
    [SerializeField] private SpriteRenderer _vfx;

    [SerializeField] private Attack _attack;
    [SerializeField] private BloodFeast _skill;
    
    [SerializeField] private float _skillCooldown;
    private float _nextSkillCastTime;
    private bool _isCasting;

    private UnitStateMachine _stateMachine;

    private void Awake() {
        base.Initialize();
    }

    /// <summary>
    /// Called by Unit.ConfigureFaction after Faction and CurrentStance (derived from Faction) are set.
    /// Responsible for initializing or updating the state machine.
    /// </summary>
    protected override void OnFactionConfigured() {
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

        if (!_isCasting)
            _stateMachine?.Update();
    }

    public void AttackComplete() {
        _stateMachine.ChangeState(new PursueState(this, _stateMachine));
    }

    public void AttackTriggered() {
        NextAttackTime = Time.time + AttackCooldown;
    }

    public void SkillTriggered() {
        _nextSkillCastTime = Time.time + _skillCooldown;
        Controllable = false;
        _isCasting = true;
    }

    public void SkillComplete() {
        _stateMachine.ChangeState(new PursueState(this, _stateMachine));
        NextAttackTime = Time.time + AttackCooldown;
        Controllable = true;
        _isCasting = false;
    }

    public override void Die() {
        base.Die();

        if (Agent != null && Agent.isOnNavMesh) {
            Agent.isStopped = true;
            Agent.enabled = false;
        }
        if (Collider != null) {
            Collider.enabled = false;
        }
        Destroy(this.gameObject, 0.1f);
    }

    public override void PerformAttack(Unit targetUnit) {
        if (_attack == null || targetUnit == null || targetUnit.IsDead) {
            _stateMachine.ChangeState(new IdleState(this, _stateMachine));
            return;
        }

        if (_nextSkillCastTime <= Time.time) {
            _skill.Initialize(baseDamage * 0.75f, null, default, this);
            _skill.Execute();
        } else {
            _attack.Initialize(baseDamage, targetUnit, KnockbackPower, this);
            _attack.Execute();
        }
    }

    public override void MoveTo(Vector2 destination) {

        _stateMachine.ChangeState(new MoveState(this, _stateMachine, destination));
    }

    public override void ForceSetTarget(Unit targetUnit) {
        base.ForceSetTarget(targetUnit);
        _stateMachine.ChangeState(new PursueState(this, _stateMachine));
    }
}
