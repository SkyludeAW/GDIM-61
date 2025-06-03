using System.Collections;
using UnityEngine;

public class Orion : Unit {
    [SerializeField] private SpriteRenderer _vfx;

    [SerializeField] private Attack _attack;
    [SerializeField] private BloodFeast _skill;

    [SerializeField] private float reviveTime = 30f;
    
    [SerializeField] private float _skillCooldown; public float SkillCooldown => _skillCooldown;
    public event StatusEvent OnCastSkill;
    private float _nextSkillCastTime; public float RemainingSkillCooldown => Mathf.Max(_nextSkillCastTime - Time.time, 0f);
    private bool _isCasting;

    private UnitStateMachine _stateMachine;

    [SerializeField] PopupMessage popupMessagePrefab;

    private void Awake() {
        base.Initialize();
        UpdateStateMachineForCurrentStance();
        _nextSkillCastTime = 0f;
    }

    /// <summary>
    /// Called by Unit.ConfigureFaction after Faction and CurrentStance (derived from Faction) are set.
    /// Responsible for initializing or updating the state machine.
    /// </summary>
    protected override void OnFactionConfigured() {
        // Debug.Log($"{gameObject.name} OnFactionConfigured. Faction: {Faction}, Stance: {CurrentStance}. Initializing/Updating SM.");
        // UpdateStateMachineForCurrentStance();
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
        OnCastSkill?.Invoke();
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

        _stateMachine.ChangeState(new IdleState(this, _stateMachine));
        baseDamage = 0f;
        KnockbackPower = 0f;
        Target = null;
        _skill.enabled = false;
        _attack.enabled = false;

        StopAllCoroutines();

        StartCoroutine(Revive());
    }

    IEnumerator Revive() {
        float regernationRate = -(maxHitPoint / reviveTime);
        _skill.AnimationListener.AttackTriggerBegin += SkillTriggered;
        _skill.AnimationListener.AnimationEnd += SkillComplete;
        _skill.AnimationListener.AnimationEnd += ReviveEnd;

        this.AnimationController.Animator.ResetTrigger("SkillEnd");
        this.AnimationController.ChangeAnimationState(AnimationController.AnimationState.Skill_1);

        while (hitPoint < maxHitPoint) {
            TakeDamage(regernationRate * Time.deltaTime);

            yield return null;
        }

        this.AnimationController.Animator.SetTrigger("SkillEnd");

        Initialize();
    }

    private void ReviveEnd() {
        Instantiate(popupMessagePrefab, transform.position, Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f))).SetUpAndActivate("孩子们我回来了！", new Vector2(0f, Random.Range(0.01f, 0.1f)), Random.Range(5f, 10f));

        OnCastSkill?.Invoke();

        _skill.enabled = true;
        _attack.enabled = true;
        _nextSkillCastTime = Time.time + _skillCooldown;
        _skill.AnimationListener.AttackTriggerBegin -= SkillTriggered;
        _skill.AnimationListener.AnimationEnd -= SkillComplete;
        _skill.AnimationListener.AnimationEnd -= ReviveEnd;
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
