using UnityEngine;

public class DeployableAreaLocator : MonoBehaviour {
    public static DeployableAreaLocator Instance { get; private set; }

    [SerializeField] CompositeCollider2D deployableArea;
    public CompositeCollider2D DeployableArea => deployableArea;
    [SerializeField] OutlineDrawer outlineDrawer;
    public OutlineDrawer OutlineDrawer => outlineDrawer;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }

        deployableArea ??= GetComponent<CompositeCollider2D>();
        outlineDrawer ??= GetComponent<OutlineDrawer>();
    }
}
