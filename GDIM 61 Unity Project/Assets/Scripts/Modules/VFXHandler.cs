using UnityEngine;

public class VFXHandler : MonoBehaviour {
    [SerializeField] private Animator animator;
    public Transform Target;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (Target != null) {
            transform.position = Target.position;
        }
    }

    public void PlayAnimation(string name = null) {
        if (animator != null) {
            if (name != null)
                animator.Play(name);
        }
    }
}
