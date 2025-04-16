using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq.Expressions;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

public abstract class Unit : MonoBehaviour {
    // Color of four factions (blue, red, yellow, purple respectively)
    // 四阵营分别为蓝色、红色、黄色与紫色
    public static readonly Color[] FACTION_COLORS = { new Color(0.282f, 0.369f, 0.859f), new Color(0.859f, 0.302f, 0.282f), new Color(0.749f, 0.722f, 0.345f), new Color(0.545f, 0.353f, 0.678f) };

    #region Member Variables
    // Essential attributes
    // 核心属性值
    [SerializeField] protected float HitPoint;
    [SerializeField] protected float MaxHitPoint;
    [SerializeField] protected float Speed = 1f;
    [SerializeField] protected float BaseDamage;
    [SerializeField] protected float AttackCooldown;
    [SerializeField] protected float NextAttackTime;
    [SerializeField] protected float KnockbackResistance = 0f;
    [SerializeField] protected bool IsInvincible = false;

    // Control-related
    // 单位控制相关
    public bool Selectable = true;
    public bool Controllable = false;
    public int Faction { get; private set; }
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
    [field: SerializeField] public Collider2D Collider { get; protected set; }
    #endregion

    #region Methods
    // Overridable initialization of hitpoint and other core attributes; to be run every time a new unit is spawned
    // 生命值以及其他核心属性的初始化；每次某单位生成时都应调用一遍
    protected virtual void Initialize() {
        HitPoint = MaxHitPoint;
        HealthUI.SetHealth(HitPoint / MaxHitPoint);
        if (Agent != null) {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            Agent.speed = Speed;
            Agent.isStopped = true;
        }
    }

    public void SetFaction(int faction) {
        this.Faction = faction;
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
                Agent.isStopped = false;
            }
        }
    }

    public void SetTarget(Unit targetUnit) {
        if (Controllable && Agent != null) {
            Target = targetUnit;
            Agent.isStopped = false;
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
                RB.AddForce(force * Mathf.Min(1f - KnockbackResistance, 0));
            if (HitPoint <= 0f)
                Die();
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
    #endregion
}
