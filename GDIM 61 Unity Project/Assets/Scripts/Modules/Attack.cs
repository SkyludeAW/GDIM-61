using UnityEngine;
using UnityEngine.Events;

public abstract class Attack : MonoBehaviour {
    protected float _damage;
    protected Vector2 _knockback;
    protected Unit _target;
    protected Unit _origin;

    public virtual void Initialize(float damage, Unit target, Vector2 force = default, Unit origin = null) {
        _damage = damage;
        _knockback = force;
        _target = target;
        _origin = origin;
    }
    public abstract void Execute();
    public UnityEvent AttackComplete;
}
