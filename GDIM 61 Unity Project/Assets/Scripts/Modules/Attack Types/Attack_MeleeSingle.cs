using UnityEngine;

public class Attack_MeleeSingle : Attack {
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
        if (yDifferenceWithTarget <= 0) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Forward, true);
        } else if (yDifferenceWithTarget > 0) {
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

            if (audioSource != null) {
                audioSource.Stop();
                audioSource.volume = Mathf.Lerp(1f, 0f, Vector3.Distance(CameraLocator.Instance.transform.position, transform.position) / audioRadius);
                audioSource.pitch = Random.Range(0.8f, 1.6f);
                audioSource.Play();
            }
        }
    }
}