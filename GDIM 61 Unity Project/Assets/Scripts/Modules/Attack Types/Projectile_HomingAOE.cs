using UnityEngine;

[RequireComponent (typeof(Collider2D), typeof(Rigidbody2D))]
public class Projectile_HomingAoE : Projecile {
    [SerializeField] new Collider2D collider;
    [SerializeField] float impactRadius;
    Vector3 direction;
    Vector3 targetPos;

    [SerializeField] VFXHandler vfx;
    [SerializeField] string vfxName;
    [SerializeField] AnimationListener listener;

    [SerializeField] private float audioRadius = 25f;
    [SerializeField] private AudioSource audioSource;

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

    public override void Initialize(float damage, Transform target, Unit origin = null, float knockback = default) {
        base.Initialize(damage, target, origin);
        collider.enabled = true;
        activated = false;
        if (knockback != default)
            this.knockback = knockback;
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
        if (activated)
            return;

        activated = true;
        vfx?.PlayAnimation(vfxName);

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, impactRadius, tangibleLayers)) {
            Unit unit = hit.GetComponent<Unit>();
            if (unit == null) continue;
            if (origin != null && unit.Faction == origin.Faction) continue;
            Vector3 distanceFromCenter = unit.transform.position - transform.position;
            unit.TakeDamage(damage, Mathf.Lerp(Mathf.Max(knockback, 0f), Mathf.Min(knockback, 0f), distanceFromCenter.magnitude / impactRadius) * distanceFromCenter.normalized, origin);
        }

        if (audioSource != null) {
            audioSource.Stop();
            audioSource.volume = Mathf.Lerp(1.6f, 0f, Vector3.Distance(CameraLocator.Instance.transform.position, transform.position) / audioRadius);
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.Play();
        }

        if (vfx == null) Purge();
    }

    protected override void Purge() => Destroy(gameObject, 0.6f);

    protected override void UpdatePosition() {
        Vector3 distance = targetPos - transform.position;
        float fixedSpeed = speed * Time.fixedDeltaTime;
        direction = distance.normalized;

        if (distance.sqrMagnitude <= 0.01f || distance.magnitude <= fixedSpeed) {
            OnHit();
            return;
        }

        if (target != null)
            targetPos = target.position;

        transform.position += direction * fixedSpeed;
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
            OnHit();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
