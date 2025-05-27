using UnityEngine;

public abstract class Projecile : MonoBehaviour {
    [SerializeField] protected float damage;
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected float knockback;
    [SerializeField] protected LayerMask tangibleLayers;
    protected Unit origin;
    protected Transform target;

    public virtual void Initialize(float damage, Transform target, Unit origin) {
        this.damage = damage;
        this.target = target;
        this.origin = origin;
    }

    public virtual void Initialize(float damage, Transform target, Unit origin, float knockback) {
        Initialize(damage, target, origin);
        this.knockback = knockback;
    }

    public virtual Projecile SetOrigin(Unit origin) {
        this.origin = origin;
        return this;
    }

    public virtual Projecile SetDamage(float damage) {
        this.damage = damage;
        return this;
    }

    public virtual Projecile SetTarget(Transform target) {
        this.target = target; 
        return this;
    }

    public virtual Projecile SetSpeed(float speed) {
        this.speed = speed;
        return this;
    }

    public virtual Projecile SetTangibleLayers(LayerMask tangibleLayers) {
        this.tangibleLayers = tangibleLayers;
        return this;
    }

    protected virtual void FixedUpdate() {
        UpdatePosition();
    }

    protected virtual void Purge() => Destroy(gameObject);

    protected abstract void UpdatePosition();
    protected abstract void OnHit();
}
