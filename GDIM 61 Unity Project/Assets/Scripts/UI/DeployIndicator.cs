using UnityEngine;

public class DeployIndicator : MonoBehaviour{
    public SpriteRenderer SR {  get; private set; }

    private void Awake() {
        SR = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        transform.position = GameController.GetMouseWorldPosition();
    }
}
