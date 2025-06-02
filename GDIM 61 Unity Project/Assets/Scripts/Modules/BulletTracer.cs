using System.Collections;
using UnityEngine;

public class BulletTracer : MonoBehaviour {
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float startWidth = 0.1f, endWidth = 0.02f;
    [SerializeField] float fadeDuration = 0.5f;
    Coroutine fadeCoroutine;
    Vector3[] positions = new Vector3[2];

    private void Awake() {
        lineRenderer ??= GetComponent<LineRenderer>();
    }

    public void Trace(Vector2 start, Vector2 end) {
        positions[0] = start;
        positions[1] = end;
        lineRenderer.SetPositions(positions);
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        gameObject.SetActive(true);
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        float elapsed = 0f;

        while (elapsed < fadeDuration) {
            float normalizedElapsed = elapsed / fadeDuration;
            lineRenderer.startWidth = Mathf.Lerp(startWidth, 0f, normalizedElapsed);
            lineRenderer.endWidth = Mathf.Lerp(endWidth, 0f, normalizedElapsed);

            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.startWidth = 0f;
        lineRenderer.endWidth = 0f;
        gameObject.SetActive(false);
    }
}
