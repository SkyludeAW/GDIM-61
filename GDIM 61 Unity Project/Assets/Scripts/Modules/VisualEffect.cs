using UnityEngine;

public class VisualEffect : MonoBehaviour {
    [SerializeField] private Animator animator;
    public Transform target;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (target != null) {
            transform.position = target.position;
        }
    }

    public void PlayAnimation(string name = null) {
        if (animator != null) {
            if (name != null)
                animator.Play(name);
        }
    }
}
