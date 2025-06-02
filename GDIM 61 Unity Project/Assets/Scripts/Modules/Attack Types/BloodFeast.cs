using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class BloodFeast : Attack {

    [SerializeField] private AnimationController _animationController;
    [SerializeField] private AnimationListener _animationListener; public AnimationListener AnimationListener => _animationListener;
    public float Radius = 5f;
    public float Duration = 6f;
    public float SkillForce = 500f;
    private LayerMask _unitLayer;

    [SerializeField] private float audioRadius = 25f;
    [SerializeField] private AudioSource audioSource;

    private void Awake() {
        //_unitLayer = LayerMask.NameToLayer("Unit");
        _unitLayer = -1;
    }

    public override void Initialize(float damage, Unit target = null, float force = 0, Unit origin = null) {
        base.Initialize(damage, target, force, origin);

        _damage *= 0.75f;
    }

    public override void Execute() {
        _animationListener.AttackTriggerEnd += SkillEnd;
        _animationListener.AttackTrigger += SkillTrigger;
        _animationListener.AttackTriggerBegin += SkillBegin;
        _animationListener.AnimationEnd += AnimationComplete;

        StartCoroutine(BeginSkillAnimation());
    }

    private IEnumerator BeginSkillAnimation() {
        _animationController.Animator.ResetTrigger("SkillEnd");
        _animationController.ChangeAnimationState(AnimationController.AnimationState.Skill_1);

        yield return new WaitForSeconds(Duration);

        _animationController.Animator.SetTrigger("SkillEnd");
    }

    private void SkillBegin() {
        AttackTriggered?.Invoke();
    }

    private void SkillTrigger() {
        if (audioSource != null) {
            audioSource.Stop();
            audioSource.volume = Mathf.Lerp(1f, 0f, Vector3.Distance(CameraLocator.Instance.transform.position, transform.position) / audioRadius);
            audioSource.pitch = Random.Range(0.9f, 1.2f);
            audioSource.Play();
        }

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, Radius, _unitLayer)) {
            if (!hit.TryGetComponent<Unit>(out Unit unit))
                continue;
            if (_origin != null && unit.Faction == _origin.Faction)
                continue;
            Vector3 distanceFromCenter = unit.transform.position - transform.position;
            unit.TakeDamage(_damage, Mathf.Lerp(Mathf.Max(-SkillForce, 0f), Mathf.Min(-SkillForce, 0f), distanceFromCenter.magnitude / Radius) * distanceFromCenter.normalized, _origin);
            _origin?.TakeDamage(-_damage * 0.5f);
        }
    }

    private void SkillEnd() {
        float finalSkillForce = SkillForce * 5f;
        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, Radius, _unitLayer)) {
            if (!hit.TryGetComponent<Unit>(out Unit unit))
                continue;
            if (_origin != null && unit.Faction == _origin.Faction)
                continue;
            Vector3 distanceFromCenter = unit.transform.position - transform.position;
            unit.TakeDamage(_damage * 5, Mathf.Lerp(Mathf.Max(finalSkillForce, 0f), Mathf.Min(finalSkillForce, finalSkillForce * 0.25f), distanceFromCenter.magnitude / Radius) * distanceFromCenter.normalized, _origin);
        }
        _origin?.TakeDamage(-_damage * 5f);
    }

    private void AnimationComplete() {
        _animationListener.AttackTriggerEnd -= SkillEnd;
        _animationListener.AttackTrigger -= SkillTrigger;
        _animationListener.AttackTriggerBegin -= SkillBegin;
        _animationListener.AnimationEnd -= AnimationComplete;
        AttackComplete?.Invoke();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
