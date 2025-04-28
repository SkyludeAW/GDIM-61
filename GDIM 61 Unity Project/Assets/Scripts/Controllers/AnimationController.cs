using UnityEngine;

public class AnimationController : MonoBehaviour {
    [SerializeField] private Animator _animator;
    [field:SerializeField] public SpriteRenderer SR { get; private set; }

    public bool CanChangeAnimationState = true;
    private AnimationState _currentState;

    public enum AnimationState {
        Idle,
        Moving_Forward, Moving_Backward, 
        Attacking_Forward, Attacking_Backward
    }

    [SerializeField] private AnimationClip[] _animations = new AnimationClip[5];

    public void ChangeAnimationState(AnimationState targetState, bool forceRestart = false) {
        if (CanChangeAnimationState && targetState >= 0 && (int) targetState < _animations.Length) {
            if (_animator != null) {
                string animationName = _animations[(int)targetState].ToString();
                if (forceRestart) {
                    _animator.Play(animationName.Remove(animationName.Length - " (UnityEngine.AnimationClip)".Length), -1, 0f);
                } else {
                    _animator.Play(animationName.Remove(animationName.Length - " (UnityEngine.AnimationClip)".Length));
                }
                _currentState = targetState;
            }
        }
    }
}
