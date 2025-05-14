using UnityEngine;

public class Attack_RangedInstaHit : Attack {
    [SerializeField] private VFXHandler vfx;
    [SerializeField] private AnimationController _animationController;
    [SerializeField] private AnimationListener _animationListener;

    public override void Execute() {
        _animationListener.AnimationEnd += AttackAnimationComplete;
        _animationListener.AttackTrigger += TargetHit;
        float yDifferenceWithTarget = _target.transform.position.y - (_origin != null ? _origin.transform.position.y : transform.position.y);
        if (yDifferenceWithTarget < 0) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Forward, true);
        } else if (yDifferenceWithTarget > 0) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Backward, true);
        }
    }

    private void AttackAnimationComplete() {
        AttackComplete?.Invoke();
        _animationListener.AnimationEnd -= AttackAnimationComplete;
        _animationListener.AttackTrigger -= TargetHit;
    }

    private void TargetHit() {
        if (_target != null && !_target.IsDead) {
            _target.TakeDamage(_damage, _knockback, _origin);
            vfx.gameObject.SetActive(true);
            vfx.Target = _target.transform;
            vfx.PlayAnimation("Blood Spill - Pierce");
        }
    }
}
