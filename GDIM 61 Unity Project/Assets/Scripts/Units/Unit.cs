using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public abstract class Unit : MonoBehaviour {
    public static readonly Color[] FACTION_COLORS = {
        new Color(0.282f, 0.369f, 0.859f), new Color(0.859f, 0.302f, 0.282f),
        new Color(0.749f, 0.722f, 0.345f), new Color(0.545f, 0.353f, 0.678f)
    };

    public enum Stance { Offensive, Defensive }

    #region Member Variables
    [SerializeField] protected Card Base; public Card BaseCard => Base;
    [SerializeField] protected float hitPoint; public float Hitpoint => hitPoint;
    [SerializeField] protected float maxHitPoint; public float MaxHitPoint => maxHitPoint;
    [SerializeField] protected float speed = 1f; public float Speed => speed;
    [SerializeField] protected float baseDamage; public float Damage => baseDamage;
    [SerializeField] protected float AttackRange; public float Range => AttackRange;
    [SerializeField] protected float AttackCooldown; public float Cooldown => AttackCooldown;
    [field: SerializeField] public float NextAttackTime { get; set; }
    [SerializeField] protected float KnockbackPower; public float Knockback => KnockbackPower;
    [SerializeField] protected float KnockbackResistance = 0f;
    [SerializeField] protected bool IsInvincible = false;
    [field: SerializeField] public bool IsDead { get; protected set; } = false;
    public delegate void StatusEvent();
    public event StatusEvent OnDie;
    public event StatusEvent OnTakeDamage;

    [field: SerializeField] public Vector3 PatrolCenter { get; set; }
    [field: SerializeField] public float AggroRadius { get; set; } = 10f;
    [field: SerializeField] public Attack Attack { get; protected set; }

    public bool Selectable = true;
    public bool Controllable = false;

    [Tooltip("The faction of the unit. Set via ConfigureFaction.")]
    [field: SerializeField] public int Faction { get; private set; }

    [Tooltip("The current target of this unit.")]
    public Unit Target;

    [Tooltip("True if the target was manually set by the player and should not be overridden by AI scans.")]
    public bool IsTargetLocked;

    public LayerMask UnpierceableLayers;
    [SerializeField] public NavMeshAgent Agent;
    [SerializeField] protected SpriteRenderer SR; public SpriteRenderer SpriteRenderer => SR;
    [SerializeField] protected SpriteRenderer SelectionSR;
    [SerializeField] protected HealthBarUI HealthUI;
    [SerializeField] protected Rigidbody2D RB;
    [field: SerializeField] public Collider2D Collider { get; protected set; }

    [Header("State Machine Related")]
    public Stance CurrentStance { get; protected set; } = Stance.Offensive;
    [SerializeField] public AnimationController AnimationController;

    protected Coroutine hurtCoroutine;
    #endregion

    #region Methods
    protected virtual void Initialize() {
        if (Base != null) {
            maxHitPoint = Base.HitPoint;
            baseDamage = Base.Damage;
            AttackCooldown = Base.AttackCooldown;
            speed = Base.Speed;
            KnockbackPower = Base.KnockbackPower;
            KnockbackResistance = Base.KnockbackResistance;
            AttackRange = Base.AttackRange;
        }
        hitPoint = maxHitPoint;
        IsDead = false;
        if (HealthUI != null) {
            HealthUI.SetHealth(hitPoint / maxHitPoint);
        }
        if (PatrolCenter == Vector3.zero) {
            PatrolCenter = transform.position;
        }
        // Inside Unit.cs Initialize() method:
        if (Agent != null) {
            Agent.enabled = true;
            Agent.updateRotation = false; // This line prevents the NavMeshAgent from rotating the Unit's transform.
            Agent.updateUpAxis = false;   // This is also important for 2D NavMesh setups, especially with NavMeshPlus.
            Agent.speed = speed;
            Agent.isStopped = false;
        }
        if (Collider != null) {
            Collider.enabled = true;
        }
        ConfigureFaction(Faction);
    }

    public void ConfigureFaction(int newFaction) {
        UnitsManager.Instance.UnregisterUnit(this);
        this.Faction = newFaction;

        if (UnitsManager.Instance != null && Application.isPlaying) {
            UnitsManager.Instance.RegisterUnit(this);
        }

        if (SelectionSR != null) {
            try {
                SelectionSR.color = FACTION_COLORS[this.Faction];
            } catch (IndexOutOfRangeException) {
                SelectionSR.color = Color.gray;
            }
        }

        OnFactionConfigured();
    }

    protected virtual void OnFactionConfigured() { }

    public virtual void SetStance(Stance newStance) {
        if (this.CurrentStance == newStance)
            return;

        this.CurrentStance = newStance;
        if (this.CurrentStance == Stance.Defensive) {
            if (Application.isPlaying)
                PatrolCenter = transform.position;
        }
        OnStanceChanged();
    }

    protected virtual void OnStanceChanged() { }

    /// <summary>
    /// Sets a destination for the unit if it's controllable. Clears any locked target.
    /// </summary>
    public void SetTargetDestination(Vector2 targetDestination) {
        if (Controllable) {
            Target = null;
            IsTargetLocked = false; // NEW: Moving to a destination unlocks the target
            if (Agent != null) {
                Agent.SetDestination(new Vector3(targetDestination.x, targetDestination.y, transform.position.z));
                Agent.isStopped = false;
            }
        }
    }

    public abstract void MoveTo(Vector2 destination);

    /// <summary>
    /// Manually sets a target unit for this unit if it's controllable and locks it.
    /// </summary>
    public virtual void ForceSetTarget(Unit targetUnit) {
        if (Controllable && Agent != null) {
            Target = targetUnit;
            IsTargetLocked = true; // NEW: Manually setting a target locks it
            Agent.isStopped = false;
        }
    }

    public void TrySetTarget(Unit targetUnit) {
        if (IsTargetLocked) {
            if (Target == null || Target.IsDead) {
                IsTargetLocked = false;
            } else
                return;
        }

        Target = targetUnit;
        if (Agent != null)
            Agent.isStopped = false;
    }

    /// <summary>
    /// Finds and sets the closest enemy unit as the target. Does NOT lock the target.
    /// </summary>
    public void FindAndSetClosestTarget(int targetFaction, float searchRadius = 0f) {
        Unit closestEnemy = null;
        float minDistanceSqr = float.MaxValue;
        if (UnitsManager.Instance != null) {
            foreach (Unit unit in UnitsManager.Instance.GetUnitsInFaction(targetFaction)) {
                if (unit != null && unit != this && !unit.IsDead && !unit.IsInvincible) {
                    float distanceSqr = (unit.transform.position - this.transform.position).sqrMagnitude;
                    if (searchRadius > 0 && distanceSqr > searchRadius * searchRadius) {
                        continue;
                    }
                    if (distanceSqr < minDistanceSqr) {
                        minDistanceSqr = distanceSqr;
                        closestEnemy = unit;
                    }
                }
            }
        }
        // This method does not set IsTargetLocked to false, allowing a locked target to persist
        // until it's cleared or a new manual command is given.
        // However, if there's no locked target, it will freely update to the nearest.
        if (!IsTargetLocked) {
            this.Target = closestEnemy;
            if (this.Target != null && Agent != null && Agent.enabled) {
                Agent.isStopped = false;
            }
        } else {
            // If the target is locked, we only clear it if it dies.
            if (Target == null || Target.IsDead) {
                IsTargetLocked = false;
                Target = closestEnemy; // Find a new target after locked one dies.
            }
        }
    }

    public void Initialize(float maxHP, float attack) { Initialize(); maxHitPoint = maxHP; hitPoint = maxHitPoint; baseDamage = attack; if (HealthUI != null) { HealthUI.SetHealth(hitPoint / maxHitPoint); } }
    public void SetSelectionActive(bool selected) { if (SelectionSR == null) return; if (selected) { SelectionSR.color = Color.green; } else { try { SelectionSR.color = FACTION_COLORS[Faction]; } catch (IndexOutOfRangeException) { SelectionSR.color = Color.gray; } } }
    public virtual void TakeDamage(float damage, Vector2 force = default, Unit origin = null) { 
        if (damage > 0 && IsInvincible) return; 
        if (damage != 0) { 
            if (hurtCoroutine != null)
                StopCoroutine(hurtCoroutine);

            hurtCoroutine = StartCoroutine(Hurt(damage));

            hitPoint -= damage;

            if (HealthUI != null) {
                HealthUI.SetHealth(hitPoint / maxHitPoint);
            }

            OnTakeDamage?.Invoke();
        } 
        if (RB != null && force != Vector2.zero) { 
            RB.AddForce(force * Mathf.Max(1f - KnockbackResistance, 0f)); 
        } 
        if (hitPoint <= 0f) { 
            hitPoint = 0f; 
            IsDead = true; 
            Die(); 
        } else if (hitPoint > maxHitPoint) {
            hitPoint = maxHitPoint;
        }
    }

    public virtual void Die() {
        IsDead = true;
        OnDie?.Invoke();
    }

    protected virtual IEnumerator Hurt(float damageTaken = 0, float hurtDuration = 0.25f) {
        float elapsed = 0f;

        if (damageTaken > 0f) {
            while (elapsed < hurtDuration) {
                SR.color = new Color(1f, Mathf.Min(elapsed / hurtDuration, 1f), Mathf.Min(elapsed / hurtDuration, 1f));
                elapsed += Time.deltaTime;
                yield return null;
            }
        } else if (damageTaken < 0f) {
            while (elapsed < hurtDuration) {
                SR.color = new Color(Mathf.Min(elapsed / hurtDuration, 1f), 1f, Mathf.Min(elapsed / hurtDuration, 1f));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        SR.color = Color.white;
    }

    protected virtual void OnDrawGizmosSelected() { 
        if (Agent != null && Agent.hasPath) { 
            Gizmos.color = Color.yellow; 
            Vector3[] corners = Agent.path.corners; 
            for (int i = 0; i < corners.Length - 1; i++) { 
                Gizmos.DrawLine(corners[i], corners[i + 1]); 
            } 
        }

        if (CurrentStance == Stance.Defensive) {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, AggroRadius);
        }

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, AttackRange); 
    }
    protected virtual void OnEnable() { if (UnitsManager.Instance != null && Application.isPlaying) { UnitsManager.Instance.RegisterUnit(this); } if (PatrolCenter == Vector3.zero && Application.isPlaying) { PatrolCenter = transform.position; } }
    protected virtual void OnDisable() { if (UnitsManager.Instance != null && Application.isPlaying) { UnitsManager.Instance.UnregisterUnit(this); } }
    public abstract void PerformAttack(Unit targetUnit);
    #endregion
}