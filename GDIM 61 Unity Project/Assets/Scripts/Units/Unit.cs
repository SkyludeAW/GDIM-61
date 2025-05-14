using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq.Expressions;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;
using System.Linq;

public abstract class Unit : MonoBehaviour {
    // Color of four factions (blue, red, yellow, purple respectively)
    // 四阵营分别为蓝色、红色、黄色与紫色
    public static readonly Color[] FACTION_COLORS = { new Color(0.282f, 0.369f, 0.859f), new Color(0.859f, 0.302f, 0.282f), new Color(0.749f, 0.722f, 0.345f), new Color(0.545f, 0.353f, 0.678f) };

    #region Member Variables
    // Essential attributes
    // 核心属性值
    [SerializeField] protected Card Base;
    [SerializeField] protected float HitPoint;
    [SerializeField] protected float MaxHitPoint;
    [SerializeField] protected float Speed = 1f;
    [SerializeField] protected float BaseDamage;
    [SerializeField] protected float AttackRange;
    [SerializeField] protected float AttackCooldown;
    [SerializeField] protected float NextAttackTime;
    [SerializeField] protected float KnockbackPower;
    [SerializeField] protected float KnockbackResistance = 0f;
    [SerializeField] protected bool IsInvincible = false;
    [field:SerializeField] public bool IsDead { get; protected set; } = false;

    // Control-related
    // 单位控制相关
    public bool Selectable = true;
    public bool Controllable = false;
    [field:SerializeField] public int Faction { get; private set; }
    public Unit Target;
    [SerializeField] protected NavMeshAgent Agent;

    // Visuals
    // 视觉贴图相关
    [SerializeField] protected SpriteRenderer SR;
    [SerializeField] protected SpriteRenderer SelectionSR;
    [SerializeField] protected HealthBarUI HealthUI;

    // Physics
    // 物理效果相关
    [SerializeField] protected Rigidbody2D RB;
    [field:SerializeField] public Collider2D Collider { get; protected set; }
    #endregion

    #region Methods
    // Overridable initialization of hitpoint and other core attributes; to be run every time a new unit is spawned
    // 生命值以及其他核心属性的初始化；每次某单位生成时都应调用一遍
    protected virtual void Initialize() {
        if (Base != null) {
            MaxHitPoint = Base.HitPoint;
            BaseDamage = Base.Damage;
            AttackCooldown = Base.AttackCooldown;
            Speed = Base.Speed;
            KnockbackPower = Base.KnockbackPower;
            KnockbackResistance = Base.KnockbackResistance;
            AttackRange = Base.AttackRange;
        }
        HitPoint = MaxHitPoint;
        HealthUI.SetHealth(HitPoint / MaxHitPoint);
        SetFaction(Faction);
        if (Agent != null) {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            Agent.speed = Speed;
            Agent.isStopped = false;
        }
    }

    public void Initialize(float maxHP, float attack) {
        Initialize();
        MaxHitPoint = maxHP;
        HitPoint = MaxHitPoint;
        BaseDamage = attack;
    }

    private void OnValidate() {
        if (Application.isPlaying) {
            SetFaction(Faction);
        }
    }

    public void SetFaction(int faction) {
        if (UnitsManager.Instance == null)
            return;
        if (UnitsManager.Instance.GetUnitsInFaction(Faction).Contains(this))
            UnitsManager.Instance.UnregisterUnit(this);
        this.Faction = faction;
        UnitsManager.Instance.RegisterUnit(this);

        try {
            SelectionSR.color = FACTION_COLORS[Faction];
        } catch(IndexOutOfRangeException) {
            SelectionSR.color = Color.gray;
        }
    }

    public void SetSelectionActive(bool selected) {
        if (selected) {
            SelectionSR.color = Color.green;
        } else {
            SelectionSR.color = FACTION_COLORS[Faction];
        }
    }

    public void SetTargetDestination(Vector2 targetDestination) {
        if (Controllable) {
            Target = null;
            if (Agent != null) {
                Agent.SetDestination(targetDestination);
                /*
                print("Desired Destination:" + targetDestination);
                print("Destination Set Successfully?" + Agent.SetDestination(targetDestination));
                print("Current Destination:" + Agent.destination);
                print("Agent isStopped?" + Agent.isStopped);
                print("Agent hasPath?" + Agent.hasPath);
                print(Agent.isOnNavMesh);
                print(Agent.pathStatus);
                */
                Agent.isStopped = false;
            }
        }
    }

    IEnumerator a() {
        yield return new WaitForSeconds(5);
    }

    public void SetTarget(Unit targetUnit) {
        if (Controllable && Agent != null) {
            Target = targetUnit;
            Agent.isStopped = false;
        }
    }

    // For AI targeting units
    protected void FindAndSetClosestTarget(int targetFaction) {
        Unit closestEnemy = null;
        float minDistanceSqr = float.MaxValue;

        if (UnitsManager.Instance != null) {
            // Use the manager to get only the relevant faction units
            foreach (Unit unit in UnitsManager.Instance.GetUnitsInFaction(targetFaction)) {
                // Check if the unit is valid, not itself, and is alive
                // GetUnitsInFaction might contain nulls if Purge wasn't run recently after destruction
                if (unit != null && unit != this && !unit.IsDead) {
                    float distanceSqr = (unit.transform.position - this.transform.position).sqrMagnitude;
                    if (distanceSqr < minDistanceSqr) {
                        minDistanceSqr = distanceSqr;
                        closestEnemy = unit;
                    }
                }
            }
        } else {
            Debug.LogWarning("UnitsManager instance not found during AI target scan.");
        }

        if (this.Target != closestEnemy) {
            this.Target = closestEnemy;
            if (this.Target != null && Agent != null) {
                Agent.isStopped = false;
            }
        }
    }

    // 受击
    public virtual void TakeDamage(float damage, Vector2 force = default, Unit origin = null) {
        if (!IsInvincible) {
            if (damage > 0)
                StartCoroutine(Hurt(0.25f));
            HitPoint -= damage;
            HealthUI.SetHealth(HitPoint / MaxHitPoint);
            if (RB != null)
                RB.AddForce(force * Mathf.Max(1f - KnockbackResistance, 0));
            if (HitPoint <= 0f) {
                IsDead = true;
                Die();
            }
        } 
    }


    // 似了
    public abstract void Die();

    // Visual effect of turning red and gradually fading back when the entity is hit
    // 受击后的红温效果
    protected virtual IEnumerator Hurt(float hurtDuration) {
        float elapsed = 0f;

        while (elapsed < hurtDuration) {
            SR.color = new Color(1f, Mathf.Min(elapsed / hurtDuration, 1f), Mathf.Min(elapsed / hurtDuration, 1f));
            elapsed += Time.deltaTime;
            yield return null;
        }
        SR.color = Color.white;
    }

    // Visualization of the NavMesh Agent's path
    void OnDrawGizmos() {
        if (Agent == null || Agent.path == null)
            return;

        Gizmos.color = Color.yellow;
        Vector3[] corners = Agent.path.corners;

        for (int i = 0; i < corners.Length - 1; i++) {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
    protected virtual void OnEnable() {
        // Ensure the Instance exists before trying to register
        if (UnitsManager.Instance != null) {
            UnitsManager.Instance.RegisterUnit(this);
        }
        // Optional: else Debug.LogError("UnitsManager not found during OnEnable!"); 
        // This might happen if script execution order isn't set correctly.
    }

    protected virtual void OnDisable() {
        // Unregister when the GameObject is disabled or destroyed
        // Check if Instance still exists (it might be destroyed first during scene unload)
        if (UnitsManager.Instance != null) {
            UnitsManager.Instance.UnregisterUnit(this);
        }
    }
    #endregion
}
