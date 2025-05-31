using UnityEngine;

[RequireComponent (typeof(Unit))]
public class Checkpoint : MonoBehaviour {
    public float Weight = 5f;
    public float CostIncrementBoost = 0f;

    [SerializeField] Unit unit;
    [SerializeField] GameObject controlledArea;

    private void Awake() {
        unit ??= GetComponent<Unit>();
        unit.OnDie += CheckPointDestroyed;
    }

    private void Start () {
        LevelProgressManager.Instance?.RegisterCheckpoint(this);
    }

    private void CheckPointDestroyed() {
        LevelProgressManager.Instance?.ConquerCheckpoint(this, controlledArea);
    }
}
