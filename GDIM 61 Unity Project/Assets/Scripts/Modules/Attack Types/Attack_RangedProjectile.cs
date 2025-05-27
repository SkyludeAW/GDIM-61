using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Attack_RangedHomingAOE : Attack {
    [SerializeField] private Projecile _projectile;
    [SerializeField] private AnimationController _animationController;
    [SerializeField] private AnimationListener _animationListener;

    private void OnEnable() {
        _animationListener.AnimationEnd += AttackAnimationComplete;
        _animationListener.AttackTrigger += AttackTrigger;
    }

    private void OnDisable() {
        _animationListener.AnimationEnd -= AttackAnimationComplete;
        _animationListener.AttackTrigger -= AttackTrigger;
    }

    public override void Execute() {
        float yDifferenceWithTarget = _target.transform.position.y - (_origin != null ? _origin.transform.position.y : transform.position.y);

        if (yDifferenceWithTarget <= (_origin != null ? _origin.Range : 0)) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Forward, true);
        } else if (yDifferenceWithTarget > (_origin != null ? _origin.Range : 0)) {
            _animationController.ChangeAnimationState(AnimationController.AnimationState.Attacking_Backward, true);
        }
    }

    private void AttackAnimationComplete() => AttackComplete?.Invoke();

    private void AttackTrigger() {
        base.AttackTriggered?.Invoke();

        Instantiate(_projectile, transform.position, Quaternion.identity).Initialize(_damage, _target.transform, _origin);
    }
}
