using UnityEngine;

public class VFXHandler : MonoBehaviour {
    [SerializeField] private Animator animator;
    public Transform Target;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (Target != null) {
            SpriteRenderer SR = Target.GetComponentInChildren<SpriteRenderer>();
            if (SR != null) 
                transform.position = SR.bounds.center;
        }
    }

    public void PlayAnimation(string name = null) {
        if (animator != null) {
            if (name != null)
                animator.Play(name);
        }
    }
}
