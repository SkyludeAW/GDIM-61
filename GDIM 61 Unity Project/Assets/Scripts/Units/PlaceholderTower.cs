using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PlaceholderTower : Unit {

    private void Awake() {
        Initialize();
    }

    protected override void Initialize() {
        base.Initialize();
        
        foreach (var unit in GetComponentsInChildren<Unit>())
            unit.ConfigureFaction(Faction);
    }

    public override void TakeDamage(float damage, Vector2 force = default, Unit origin = null) {
        base.TakeDamage(damage, force, origin);
        Debug.Log(gameObject.name + " took " + damage + " damage from " + origin?.gameObject.name + " at " + Time.time);
    }

    // 似了
    public override void Die() {
        IsDead = true;
        Destroy(this.gameObject);
    }

    public override void PerformAttack(Unit targetUnit) {
        // throw new System.NotImplementedException();
    }
}