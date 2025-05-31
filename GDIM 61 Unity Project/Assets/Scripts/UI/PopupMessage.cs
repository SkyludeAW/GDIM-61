using System.Collections;
using TMPro;
using UnityEngine;

public class PopupMessage : MonoBehaviour {
    [SerializeField] TMP_Text textMesh;
    [SerializeField, Min(0.001f)] float lifeTime = 3f;
    Vector2 initialVelocity;

    private void Awake() {
        textMesh ??= GetComponent<TMP_Text>();
    }

    IEnumerator LifeCycle() {
        float elapsed = 0;

        while (elapsed < lifeTime) {
            float normalizedElapsed = elapsed / lifeTime;

            Vector3 velocity = Vector2.Lerp(initialVelocity, Vector2.zero, Mathf.Pow(normalizedElapsed, 0.1f));
            transform.position += velocity;

            Color previousColor = textMesh.color;
            textMesh.color = new Color(previousColor.r, previousColor.g, previousColor.b, 1 - (normalizedElapsed * normalizedElapsed));

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetUpAndActivate(string message, Vector2 velocity = default, float fontSize = 6f) {
        if (textMesh != null) {
            textMesh.text = message;
            textMesh.fontSize = fontSize;
        }

        initialVelocity = transform.rotation * velocity;

        StartCoroutine(LifeCycle());
    }
}
