using UnityEngine;
using System.Collections.Generic; // Required for using Lists

[RequireComponent(typeof(CompositeCollider2D))]
public class OutlineDrawer : MonoBehaviour {
    [SerializeField] GameObject lineRendererPrefab; // Assign a prefab with a LineRenderer component
    [SerializeField] CompositeCollider2D compositeCollider;
    private List<GameObject> activeLineRenderers = new List<GameObject>();

    void Start() {
        compositeCollider ??= GetComponent<CompositeCollider2D>();
        if (lineRendererPrefab == null) {
            Debug.LogError("LineRenderer prefab is not assigned!", this);
            enabled = false; // Disable the script if prefab is missing
            return;
        }
        DrawOutline();
    }

    // Call this method whenever the CompositeCollider2D is updated
    public void UpdateOutline() {
        ClearOutline();
        DrawOutline();
    }

    void ClearOutline() {
        foreach (GameObject lineObj in activeLineRenderers) {
            Destroy(lineObj); // Destroy the GameObject holding the LineRenderer
        }
        activeLineRenderers.Clear(); // Clear the list
    }

    void DrawOutline() {
        if (compositeCollider == null || lineRendererPrefab == null)
            return; // Safety check

        // Regenerate the geometry if needed, especially if child colliders have changed.
        // This ensures GetPath() returns the latest data.
        // Note: This can be expensive if called frequently.
        // Consider if you truly need to call this every time or only when specific changes occur.
        // compositeCollider.GenerateGeometry(); // Uncomment if you suspect paths aren't updating

        for (int i = 0; i < compositeCollider.pathCount; i++) {
            Vector2[] pathVertices = new Vector2[compositeCollider.GetPathPointCount(i)];
            compositeCollider.GetPath(i, pathVertices);

            if (pathVertices.Length < 2)
                continue;

            GameObject lineObj = Instantiate(lineRendererPrefab, transform); // Parent to this object
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            if (lineRenderer == null) {
                Debug.LogError("LineRenderer component not found on the instantiated prefab!", lineObj);
                Destroy(lineObj); // Clean up if prefab is misconfigured
                continue;
            }

            // Configure LineRenderer
            lineRenderer.positionCount = pathVertices.Length;
            lineRenderer.loop = true;

            for (int j = 0; j < pathVertices.Length; j++) {
                lineRenderer.SetPosition(j, pathVertices[j]);
            }

            // Add the new LineRenderer GameObject to our tracking list
            activeLineRenderers.Add(lineObj);

            // Optional: Customize line appearance
            // lineRenderer.startWidth = 0.1f;
            // lineRenderer.endWidth = 0.1f;
            // lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            // lineRenderer.startColor = Color.green;
            // lineRenderer.endColor = Color.green;
        }
    }

    // Optional: If you want to clear the outline when this component is destroyed or disabled
    void OnDisable() {
        ClearOutline();
    }

    void OnDestroy() {
        ClearOutline();
    }

    // --- How to Trigger UpdateOutline ---
    // You need to call UpdateOutline() when you know the CompositeCollider2D has changed.
    // Example:
    // 1. If you manually add/remove child colliders:
    //    MyCompositeColliderScript.RemoveChildCollider(someChild);
    //    MyCompositeColliderScript.GetComponent<DrawCompositeOutline>().UpdateOutline();
    //
    // 2. If the collider changes due to physics or other runtime modifications:
    //    This is trickier as CompositeCollider2D doesn't have a direct "OnUpdated" event.
    //    - You might call UpdateOutline() after performing an action that you know changes the collider.
    //    - For frequent changes, you *could* call it in Update(), but this can be inefficient.
    //      A better approach would be to only call it when a change has actually occurred.
    //
    // Example of calling it after a delay (e.g., if collider regeneration takes a frame):
    // public void RefreshOutlineAfterDelay()
    // {
    //    StartCoroutine(RefreshOutlineCoroutine());
    // }
    //
    // private System.Collections.IEnumerator RefreshOutlineCoroutine()
    // {
    //    // Wait for the end of the frame, allowing physics and collider updates to settle
    //    yield return new WaitForEndOfFrame();
    //    UpdateOutline();
    // }
}