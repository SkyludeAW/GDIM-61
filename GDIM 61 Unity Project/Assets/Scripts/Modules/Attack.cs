using UnityEngine;

public abstract class Attack {
    private float _damage;
    private Vector2 _knockback;
    private Unit _target;
    private Unit _origin;
    private float _velocity;

    public virtual void Initialize(float damage, Unit target, Vector2 force = default, Unit origin = null, float velocity = 0f) {
        _damage = damage;
        _knockback = force;
        _target = target;
        _origin = origin;
        _velocity = velocity;
    }
    public abstract void Execute();
}
