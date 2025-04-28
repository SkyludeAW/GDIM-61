using UnityEngine;

public class CollisionRepulsion : MonoBehaviour {
    [SerializeField] private float _repulsionForce = 1024f;
    [SerializeField] private float _repulsionRadius = 0.36f;

    // Used for a softer collision physics
    // 实现更加柔和的单位之间的推挤效果
    private void OnTriggerStay2D(Collider2D other) {
        Rigidbody2D otherRB = other.GetComponent<Rigidbody2D>();
        if (otherRB != null) {
            Vector2 direction = (otherRB.position - (Vector2) transform.position).normalized;

            float distance = Vector2.Distance(transform.position, otherRB.position);
            float distanceFactor = Mathf.Clamp01(1 - (distance / _repulsionRadius));

            otherRB.AddForce(direction * _repulsionForce * distanceFactor * Time.deltaTime, ForceMode2D.Force);
        }
    }
}
