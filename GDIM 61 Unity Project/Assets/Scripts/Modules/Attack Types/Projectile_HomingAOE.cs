using UnityEngine;

[RequireComponent (typeof(Collider2D), typeof(Rigidbody2D))]
public class Projectile_HomingAoE : Projecile {
    [SerializeField] new Collider2D collider;
    [SerializeField] float impactRadius;
    Vector3 direction;

    [SerializeField] VFXHandler vfx;
    [SerializeField] string vfxName;
    [SerializeField] AnimationListener listener;

    bool activated;

    private void Awake() {
        collider ??= GetComponent<Collider2D>();
        vfx ??= GetComponent<VFXHandler>();
        listener ??= GetComponent<AnimationListener>();
    }

    private void OnEnable() {
        if (listener != null) {
            listener.AnimationEnd += Purge;
        }
    }

    private void OnDisable() {
        if (listener != null) {
            listener.AnimationEnd -= Purge;
        }
    }

    public override void Initialize(float damage, Transform target, Unit origin = null) {
        base.Initialize(damage, target, origin);
        collider.enabled = true;
        activated = false;
    }

    public GameObject Initialize(float damage, Transform target, float radius, Unit origin, float knockback, float speed) {
        this.damage = damage;
        this.target = target;
        this.knockback = knockback;
        this.origin = origin;
        this.impactRadius= radius;
        this.speed = speed;
        transform.localScale.Set(radius, radius, radius);
        collider.enabled = true;
        activated = false;
        return this.gameObject;
    }

    protected override void OnHit() {
        vfx?.PlayAnimation(vfxName);

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, impactRadius, tangibleLayers)) {
            Unit unit = hit.GetComponent<Unit>();
            if (unit == null) continue;
            if (origin != null && unit.Faction == origin.Faction) continue;
            Vector3 distanceFromCenter = unit.transform.position - transform.position;
            unit.TakeDamage(damage, Mathf.Lerp(Mathf.Max(knockback, 0f), Mathf.Min(knockback, 0f), distanceFromCenter.magnitude / impactRadius) * distanceFromCenter.normalized, origin);
        }

        if (vfx == null) Purge();
    }

    protected override void UpdatePosition() {
        if ( target == null ) {
            Purge();
            return;
        }

        direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ( activated ) {
            return;
        }

        if ((tangibleLayers.value & (1 << collision.gameObject.layer)) != 0) {
            Unit unit = collision.GetComponent<Unit>();
            if (unit != null && origin != null && unit.Faction == origin.Faction || activated) {
                return;
            }
            activated = true;
            OnHit();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
