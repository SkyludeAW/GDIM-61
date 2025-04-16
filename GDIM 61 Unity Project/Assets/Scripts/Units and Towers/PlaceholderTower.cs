using UnityEngine;
using UnityEngine.AI;

public class PlaceholderTower : Unit {
    [SerializeField] private int _faction;

    private void Awake() {
        Initialize();
    }

    private void Update() {
        

        if (Input.GetKeyDown(KeyCode.E)) {
            TakeDamage(25);
        }
    }

    protected override void Initialize() {
        base.Initialize();
        SetFaction(_faction);
        Controllable = false;
    }

    // 似了
    public override void Die() {
        Destroy(this.gameObject);
    }
}