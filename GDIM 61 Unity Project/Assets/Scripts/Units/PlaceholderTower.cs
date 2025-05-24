using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlaceholderTower : Unit {

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
        Controllable = false;
        if (UnitsManager.Instance != null && !UnitsManager.Instance.GetUnitsInFaction(Faction).Contains(this)) {
            UnitsManager.Instance.RegisterUnit(this);
        }
    }

    // 似了
    public override void Die() {
        Destroy(this.gameObject);
    }

    public override void PerformAttack(Unit targetUnit) {
        // throw new System.NotImplementedException();
    }
}