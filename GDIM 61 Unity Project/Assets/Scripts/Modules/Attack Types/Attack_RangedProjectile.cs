using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Attack_RangedHomingAOE : Attack {
    [SerializeField] private Projecile _projectile;
    [SerializeField] private AnimationController _animationController;
    [SerializeField] private AnimationListener _animationListener;

    [SerializeField] private float audioRadius = 25f;
    [SerializeField] private AudioSource audioSource;

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
        if (_target == null || _target.IsDead) return;

        AttackTriggered?.Invoke();

        if (audioSource != null) {
            audioSource.Stop();
            audioSource.volume = Mathf.Lerp(1f, 0f, Vector3.Distance(CameraLocator.Instance.transform.position, transform.position) / audioRadius);
            audioSource.pitch = Random.Range(0.9f, 1.5f);
            audioSource.Play();
        }

        Instantiate(_projectile, transform.position, Quaternion.identity).Initialize(_damage, _target.transform, _origin, _knockback);
    }
}
