using UnityEngine;

public class Attack_RangedInstaHit : Attack {
    [SerializeField] private VFXHandler vfx;
    [SerializeField] private BulletTracer tracer;

    [SerializeField] private AnimationController _animationController;
    [SerializeField] private AnimationListener _animationListener;

    [SerializeField] private float audioRadius = 20f;
    [SerializeField] private AudioSource audioSource;

    private void OnEnable() {
        _animationListener.AnimationEnd += AttackAnimationComplete;
        _animationListener.AttackTrigger += TargetHit;
    }

    private void OnDisable() {
        _animationListener.AnimationEnd -= AttackAnimationComplete;
        _animationListener.AttackTrigger -= TargetHit;
    }

    public override void Execute() {
        float yDifferenceWithTarget = _target.transform.position.y - (_origin != null ? _origin.transform.position.y : transform.position.y);
        
        if (yDifferenceWithTarget <= (_origin != null ? _origin.Range : 0)) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Forward, true);
        } else if (yDifferenceWithTarget > (_origin != null ? _origin.Range : 0)) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Backward, true);
        }
    }

    private void AttackAnimationComplete() {
        AttackComplete?.Invoke();
    }

    private void TargetHit() {
        if (_target != null && !_target.IsDead) {
            AttackTriggered?.Invoke();

            Vector2 knockbackDirection = (_target.transform.position - transform.position).normalized;
            _target.TakeDamage(_damage, knockbackDirection * _knockback, _origin);

            tracer?.Trace(transform.position, (_target.SpriteRenderer == null) ? _target.transform.position : _target.SpriteRenderer.bounds.center);

            if (!(_target is PlaceholderTower || _target is NexusTower)) {
                vfx.gameObject.SetActive(true);
                vfx.Target = _target.transform;
                vfx.PlayAnimation("Blood Spill - Pierce");
            }

            if (audioSource != null) {
                audioSource.Stop();
                audioSource.volume = Mathf.Lerp(0.6f, 0f, Vector3.Distance(CameraLocator.Instance.transform.position, transform.position) / audioRadius);
                audioSource.pitch = Random.Range(0.9f, 1.2f);
                audioSource.Play();
            }
        }
    }
}
